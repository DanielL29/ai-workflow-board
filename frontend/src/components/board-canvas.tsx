"use client";

import { useMemo, useState, useEffect } from "react";
import { Background, Controls, MiniMap, ReactFlow, ReactFlowProvider, type NodeTypes } from "@xyflow/react";
import { BoardNode } from "@/components/nodes/board-node";
import { useBoardStore } from "@/store/board-store";
import type { NodeKind } from "@/lib/types";
import { joinBoardGroup, onNodeUpdated } from "@/lib/signalr";

const nodeTypes: NodeTypes = {
  boardNode: BoardNode,
};

const quickNodes: { kind: NodeKind; label: string }[] = [
  { kind: "text", label: "Text" },
  { kind: "prompt", label: "Prompt" },
  { kind: "image", label: "Image" },
  { kind: "assistant", label: "Assistant" },
];

export function BoardCanvas() {
  return (
    <ReactFlowProvider>
      <BoardCanvasInner />
    </ReactFlowProvider>
  );
}

function BoardCanvasInner() {
  const { nodes, edges, onNodesChange, onEdgesChange, onConnect, onNodeDragStop, addNode, setSelectedNodeId } = useBoardStore();
  const boardId = useBoardStore((s) => s.boardId);

  useEffect(() => {
    if (!boardId) return;
    const rawBase = process.env.NEXT_PUBLIC_API_HUB_URL ?? process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080";
    const hubOrigin = rawBase.replace(/\/api\/v\d+\/?$/, "");
    const hubUrl = hubOrigin + "/hubs/board";
    let off: (() => void) | null = null;
    (async () => {
      try {
        await joinBoardGroup(hubUrl, boardId);
        off = onNodeUpdated((payload) => {
          try {
            const { nodeId, status, output } = payload as any;
            useBoardStore.setState((state) => {
              const nodes = state.nodes.map((n) => (n.id === nodeId ? { ...n, data: { ...n.data, outputContent: output, status } } : n));
              const generatingNodeId = state.generatingNodeId === nodeId && (status === 4 || status === 5) ? null : state.generatingNodeId;
              return { nodes, generatingNodeId } as any;
            });
          } catch {
          }
        });
      } catch {
      }
    })();

    return () => off?.();
  }, [boardId]);
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number } | null>(null);

  const proOptions = useMemo(() => ({ hideAttribution: true }), []);

  return (
    <div className="relative h-full min-h-[680px] overflow-hidden rounded-[28px] border border-white/60 bg-white/50 shadow-card backdrop-blur-sm">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        nodeTypes={nodeTypes}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        onNodeDragStop={onNodeDragStop}
        onNodeClick={(_, node) => setSelectedNodeId(node.id)}
        onPaneClick={() => {
          setSelectedNodeId(null);
          setContextMenu(null);
        }}
        onPaneContextMenu={(event) => {
          event.preventDefault();
          setContextMenu({ x: event.clientX, y: event.clientY });
        }}
        fitView
        panOnScroll
        selectionOnDrag
        panOnDrag={false}
        proOptions={proOptions}
        className="bg-transparent"
      >
        <MiniMap pannable zoomable className="!rounded-2xl !border !border-stone-200 !bg-white/90" />
        <Controls className="!shadow-card" />
        <Background gap={24} size={1} color="#c8bba8" />
      </ReactFlow>

      {contextMenu ? (
        <div
          className="absolute z-20 min-w-44 rounded-2xl border border-stone-200 bg-white/95 p-2 shadow-card"
          style={{ left: contextMenu.x - 220, top: contextMenu.y - 80 }}
        >
          <div className="px-2 pb-2 text-[11px] font-semibold uppercase tracking-[0.2em] text-stone-400">
            Quick create
          </div>
          {quickNodes.map((item, index) => (
            <button
              key={item.kind}
              className="flex w-full items-center justify-between rounded-xl px-3 py-2 text-left text-sm text-stone-700 transition hover:bg-stone-100"
              onClick={async () => {
                await addNode(item.kind, { x: 160 + index * 30, y: 120 + index * 20 });
                setContextMenu(null);
              }}
              type="button"
            >
              <span>{item.label}</span>
              <span className="text-xs uppercase tracking-[0.18em] text-stone-400">{item.kind}</span>
            </button>
          ))}
        </div>
      ) : null}
    </div>
  );
}
