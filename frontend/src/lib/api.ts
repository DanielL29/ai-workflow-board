import type { ApiAssistantResponseEnvelope, ApiBoard, NodeKind } from "@/lib/types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080/api/v1";

function mapNodeKindToNumber(kind: NodeKind) {
  switch (kind) {
    case "text":
      return 1;
    case "image":
      return 2;
    case "prompt":
      return 3;
    case "assistant":
      return 4;
    case "group":
      return 5;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers: Record<string, string> = {
    ...(init?.headers as Record<string, string> ?? {}),
  };

  // Only set content-type when a body is present (avoids preflight for simple GETs)
  if (init?.body) {
    headers["Content-Type"] = "application/json";
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers,
    cache: "no-store",
  });

  if (!response.ok) {
    const body = await response.text();

    try {
      const problem = JSON.parse(body) as { detail?: string; title?: string };
      throw new Error(problem.detail || problem.title || `HTTP ${response.status}`);
    } catch {
      throw new Error(body || `HTTP ${response.status}`);
    }
  }

  // No content (204) returns empty body — avoid calling response.json() which throws
  if (response.status === 204) {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    return (undefined as any) as T;
  }

  return response.json() as Promise<T>;
}

export async function createBoard(name: string, description?: string) {
  return request<ApiBoard>("/boards", {
    method: "POST",
    body: JSON.stringify({ name, description }),
  });
}

export async function getBoard(boardId: string) {
  return request<ApiBoard>(`/boards/${boardId}`);
}

export async function createNode(boardId: string, input: {
  kind: NodeKind;
  title: string;
  content: string;
  x: number;
  y: number;
  model?: string;
}) {
  return request(`/boards/${boardId}/nodes`, {
    method: "POST",
    body: JSON.stringify({
      type: mapNodeKindToNumber(input.kind),
      title: input.title,
      content: input.content,
      x: input.x,
      y: input.y,
      model: input.model ?? null,
    }),
  });
}

export async function deleteNode(boardId: string, nodeId: string) {
  return request(`/boards/${boardId}/nodes/${nodeId}`, {
    method: "DELETE",
  });
}

export async function listBoards() {
  return request<ApiBoard[]>(`/boards`);
}

export async function createEdge(boardId: string, sourceNodeId: string, targetNodeId: string) {
  return request(`/boards/${boardId}/edges`, {
    method: "POST",
    body: JSON.stringify({ sourceNodeId, targetNodeId, label: null }),
  });
}

export async function updateNode(boardId: string, nodeId: string, input: {
  title: string;
  content: string;
  x: number;
  y: number;
  model?: string | null;
}) {
  return request(`/boards/${boardId}/nodes/${nodeId}`, {
    method: "PUT",
    body: JSON.stringify({
      title: input.title,
      content: input.content,
      x: input.x,
      y: input.y,
      model: input.model ?? null,
    }),
  });
}

export async function askAssistant(boardId: string | null, message: string) {
  return request<ApiAssistantResponseEnvelope>("/assistant/reply", {
    method: "POST",
    body: JSON.stringify({ boardId, message }),
  });
}

export async function queueNodeGeneration(boardId: string, input: {
  nodeId: string;
  provider: string;
  prompt: string;
}) {
  return request<{ jobId: string }>(`/boards/${boardId}/generations`, {
    method: "POST",
    body: JSON.stringify(input),
  });
}

export async function generateNode(boardId: string, input: {
  nodeId: string;
  provider: string;
  prompt: string;
}) {
  // Keep sync generation available if needed, but prefer queueing from frontend.
  return request(`/${"boards"}/${boardId}/nodes/${input.nodeId}/generate`, {
    method: "POST",
    body: JSON.stringify(input),
  });
}
