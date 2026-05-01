export type ApiBoard = {
  id: string;
  name: string;
  description?: string | null;
  nodes: ApiBoardNode[];
  edges: ApiBoardEdge[];
  updatedAtUtc: string;
};

export type ApiBoardNode = {
  id: string;
  type: number;
  title: string;
  content: string;
  model?: string | null;
  outputContent?: string | null;
  x: number;
  y: number;
  status: number;
};

export type ApiBoardEdge = {
  id: string;
  sourceNodeId: string;
  targetNodeId: string;
  label?: string | null;
};

export type ApiAssistantResponseEnvelope = {
  response: string;
};

export type NodeKind = "text" | "image" | "prompt" | "assistant" | "group";

export type ChatMessage = {
  id: string;
  role: "user" | "assistant";
  content: string;
};
