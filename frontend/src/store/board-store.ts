"use client";

import { addEdge, applyEdgeChanges, applyNodeChanges, type Edge, type Node, type OnConnect, type OnEdgesChange, type OnNodesChange } from "@xyflow/react";
import { create } from "zustand";
import { askAssistant, createBoard, createEdge, createNode, generateNode, getBoard, updateNode, queueNodeGeneration, deleteNode, listBoards } from "@/lib/api";
import type { ApiBoard, ApiBoardNode, ChatMessage, NodeKind } from "@/lib/types";

type BoardNodeData = {
  title: string;
  content: string;
  outputContent?: string | null;
  kind: NodeKind;
  status: number;
  model?: string | null;
};

type BoardStore = {
  boardId: string | null;
  boardName: string;
  nodes: Node<BoardNodeData>[];
  edges: Edge[];
  selectedNodeId: string | null;
  loading: boolean;
  error: string | null;
  chat: ChatMessage[];
  savingNodeId: string | null;
  generatingNodeId: string | null;
  setSelectedNodeId: (nodeId: string | null) => void;
  onNodesChange: OnNodesChange<Node<BoardNodeData>>;
  onEdgesChange: OnEdgesChange<Edge>;
  onNodeDragStop: (event: unknown, node: Node<BoardNodeData>) => Promise<void>;
  onConnect: (connection: Parameters<OnConnect>[0]) => Promise<void>;
  createNewBoard: (name: string) => Promise<void>;
  loadBoard: (boardId: string) => Promise<void>;
  addNode: (kind: NodeKind, position: { x: number; y: number }) => Promise<void>;
  updateSelectedNode: (input: { title: string; content: string; model?: string | null }) => Promise<void>;
  generateSelectedNode: () => Promise<void>;
  sendAssistantMessage: (message: string) => Promise<void>;
  deleteNode: (nodeId: string) => Promise<void>;
  listBoards: () => Promise<ApiBoard[]>;
};

function mapNodeType(type: number): NodeKind {
  switch (type) {
    case 1:
      return "text";
    case 2:
      return "image";
    case 3:
      return "prompt";
    case 4:
      return "assistant";
    case 5:
      return "group";
    default:
      return "text";
  }
}

function mapBoard(board: ApiBoard): Pick<BoardStore, "boardId" | "boardName" | "nodes" | "edges"> {
  return {
    boardId: board.id,
    boardName: board.name,
    nodes: board.nodes.map((node) => mapApiNode(node)),
    edges: board.edges.map((edge) => ({
      id: edge.id,
      source: edge.sourceNodeId,
      target: edge.targetNodeId,
      type: "smoothstep",
      animated: true,
    })),
  };
}

function mapApiNode(node: ApiBoardNode): Node<BoardNodeData> {
  return {
    id: node.id,
    type: "boardNode",
    position: { x: node.x, y: node.y },
    data: {
      title: node.title,
      content: node.content,
      outputContent: node.outputContent,
      kind: mapNodeType(node.type),
      status: node.status,
      model: node.model,
    },
  };
}

function buildNodeTemplate(kind: NodeKind) {
  switch (kind) {
    case "prompt":
      return { title: "Prompt node", content: "Describe the image or task you want the AI to perform." };
    case "image":
      return { title: "Image node", content: "Image generation prompt or asset reference." };
    case "assistant":
      return { title: "Assistant node", content: "Use the board context to refine the next step." };
    case "group":
      return { title: "Group node", content: "Cluster related ideas or nodes here." };
    default:
      return { title: "Text node", content: "Drop research notes, copy, or instructions here." };
  }
}

function buildGenerationPrompt(targetNodeId: string, nodes: Node<BoardNodeData>[], edges: Edge[]) {
  const targetNode = nodes.find((node) => node.id === targetNodeId);
  if (!targetNode) {
    return "";
  }

  const incomingNodes = edges
    .filter((edge) => edge.target === targetNodeId)
    .map((edge) => nodes.find((node) => node.id === edge.source))
    .filter((node): node is Node<BoardNodeData> => Boolean(node));

  const sections = [
    ...incomingNodes.map((node) => `${node.data.title}\n${node.data.content}`.trim()),
    `${targetNode.data.title}\n${targetNode.data.content}`.trim(),
  ]
    .map((section) => section.trim())
    .filter(Boolean);

  return sections.join("\n\n");
}

export const useBoardStore = create<BoardStore>((set, get) => ({
  boardId: null,
  boardName: "Untitled board",
  nodes: [],
  edges: [],
  selectedNodeId: null,
  loading: false,
  error: null,
  chat: [],
  savingNodeId: null,
  generatingNodeId: null,
  setSelectedNodeId: (selectedNodeId) => set({ selectedNodeId }),
  onNodesChange: (changes) => set((state) => ({ nodes: applyNodeChanges(changes, state.nodes) })),
  onEdgesChange: (changes) => set((state) => ({ edges: applyEdgeChanges(changes, state.edges) })),
  onNodeDragStop: async (_, node) => {
    const boardId = get().boardId;
    if (!boardId) {
      return;
    }

    try {
      const updatedNode = await updateNode(boardId, node.id, {
        title: String(node.data.title ?? ""),
        content: String(node.data.content ?? ""),
        x: node.position.x,
        y: node.position.y,
        model: node.data.model ?? null,
      });

      set((state) => ({
        nodes: state.nodes.map((currentNode) =>
          currentNode.id === node.id ? mapApiNode(updatedNode as ApiBoardNode) : currentNode),
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to persist node position." });
    }
  },
  onConnect: async (connection) => {
    const boardId = get().boardId;
    if (!boardId || !connection.source || !connection.target) {
      return;
    }

    const localEdge: Edge = {
      id: `${connection.source}-${connection.target}-${crypto.randomUUID()}`,
      source: connection.source,
      target: connection.target,
      type: "smoothstep",
      animated: true,
    };

    set((state) => ({ edges: addEdge(localEdge, state.edges) }));
    await createEdge(boardId, connection.source, connection.target);
  },
  createNewBoard: async (name) => {
    set({ loading: true, error: null });
    try {
      const board = await createBoard(name, "Board created from the frontend MVP.");
      set({ ...mapBoard(board), loading: false, chat: [] });
    } catch (error) {
      set({ loading: false, error: error instanceof Error ? error.message : "Failed to create board." });
    }
  },
  loadBoard: async (boardId) => {
    set({ loading: true, error: null });
    try {
      const board = await getBoard(boardId);
      set({ ...mapBoard(board), loading: false });
    } catch (error) {
      set({ loading: false, error: error instanceof Error ? error.message : "Failed to load board." });
    }
  },
  addNode: async (kind, position) => {
    const boardId = get().boardId;
    if (!boardId) {
      set({ error: "Create or load a board first." });
      return;
    }

    const template = buildNodeTemplate(kind);
    try {
      const createdNode = await createNode(boardId, {
        kind,
        title: template.title,
        content: template.content,
        x: position.x,
        y: position.y,
      });

      set((state) => ({
        nodes: [...state.nodes, mapApiNode(createdNode as ApiBoardNode)],
        error: null,
      }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to create node." });
    }
  },
  updateSelectedNode: async (input) => {
    const boardId = get().boardId;
    const selectedNodeId = get().selectedNodeId;
    const currentNode = get().nodes.find((node) => node.id === selectedNodeId);

    if (!boardId || !selectedNodeId || !currentNode) {
      set({ error: "Select a node before saving changes." });
      return;
    }

    set({ savingNodeId: selectedNodeId, error: null });

    try {
      const updatedNode = await updateNode(boardId, selectedNodeId, {
        title: input.title,
        content: input.content,
        x: currentNode.position.x,
        y: currentNode.position.y,
        model: input.model ?? null,
      });

      set((state) => ({
        nodes: state.nodes.map((node) => (node.id === selectedNodeId ? mapApiNode(updatedNode as ApiBoardNode) : node)),
        savingNodeId: null,
      }));
    } catch (error) {
      set({
        savingNodeId: null,
        error: error instanceof Error ? error.message : "Failed to update node.",
      });
    }
  },
  generateSelectedNode: async () => {
    const boardId = get().boardId;
    const selectedNodeId = get().selectedNodeId;
    const state = get();
    const selectedNode = state.nodes.find((node) => node.id === selectedNodeId);

    if (!boardId || !selectedNodeId || !selectedNode) {
      set({ error: "Select a node before generating." });
      return;
    }

    if (selectedNode.data.kind !== "image") {
      set({ error: "Generation is currently available only for image nodes." });
      return;
    }

    const prompt = buildGenerationPrompt(selectedNodeId, state.nodes, state.edges);
    if (!prompt.trim()) {
      set({ error: "Add prompt text to the image node or connect source nodes with content first." });
      return;
    }

    set({ generatingNodeId: selectedNodeId, error: null });

    try {
      await queueNodeGeneration(boardId, {
        nodeId: selectedNodeId,
        provider: selectedNode.data.model?.trim() ? `local-sd:${selectedNode.data.model.trim()}` : "local-sd",
        prompt,
      });

      // We queued the job; set node status to Queued (2) and keep `generatingNodeId`
      set((state) => ({
        nodes: state.nodes.map((n) => (n.id === selectedNodeId ? { ...n, data: { ...n.data, status: 2 } } : n)),
        error: null,
      }));
    } catch (error) {
      set({
        generatingNodeId: null,
        error: error instanceof Error ? error.message : "Failed to queue node generation.",
      });
    }
  },
  deleteNode: async (nodeId) => {
    const boardId = get().boardId;
    if (!boardId) {
      set({ error: "No board loaded." });
      return;
    }

    try {
      await deleteNode(boardId, nodeId);
      set((state) => ({ nodes: state.nodes.filter((n) => n.id !== nodeId) }));
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to delete node." });
    }
  },
  listBoards: async () => {
    try {
      return await listBoards();
    } catch (error) {
      set({ error: error instanceof Error ? error.message : "Failed to list boards." });
      return [];
    }
  },
  sendAssistantMessage: async (message) => {
    const boardId = get().boardId;
    const newUserMessage: ChatMessage = { id: crypto.randomUUID(), role: "user", content: message };

    set((state) => ({
      chat: [...state.chat, newUserMessage],
      error: null,
    }));

    try {
      const envelope = await askAssistant(boardId, message);
      let content = envelope.response;

      try {
        const parsed = JSON.parse(envelope.response) as { answer?: string };
        if (parsed.answer) {
          content = parsed.answer;
        }
      } catch {
      }

      set((state) => ({
        chat: [...state.chat, { id: crypto.randomUUID(), role: "assistant", content }],
      }));
    } catch (error) {
      set((state) => ({
        chat: [...state.chat, { id: crypto.randomUUID(), role: "assistant", content: "Assistant request failed." }],
        error: error instanceof Error ? error.message : "Assistant request failed.",
      }));
    }
  },
}));
