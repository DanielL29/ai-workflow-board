"use client";

import { useEffect, useState } from "react";
import { parseNodeOutput } from "@/lib/node-output";
import { useBoardStore } from "@/store/board-store";

export function InspectorPanel() {
  const { nodes, selectedNodeId, updateSelectedNode, savingNodeId, generateSelectedNode, generatingNodeId } = useBoardStore();
  const selectedNode = nodes.find((node) => node.id === selectedNodeId);
  const parsedOutput = parseNodeOutput(selectedNode?.data.outputContent);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [model, setModel] = useState("");

  useEffect(() => {
    setTitle(selectedNode?.data.title ?? "");
    setContent(selectedNode?.data.content ?? "");
    setModel(selectedNode?.data.model ?? "");
  }, [selectedNode?.id, selectedNode?.data.title, selectedNode?.data.content, selectedNode?.data.model]);

  return (
    <section className="rounded-[24px] border border-white/70 bg-white/75 p-4 shadow-card backdrop-blur-sm">
      <div className="text-xs uppercase tracking-[0.24em] text-stone-400">Inspector</div>
      {selectedNode ? (
        <form
          className="mt-3 space-y-4"
          onSubmit={async (event) => {
            event.preventDefault();
            await updateSelectedNode({ title, content, model });
          }}
        >
          <div>
            <div className="text-sm font-semibold text-ink">Edit node</div>
            <div className="text-xs uppercase tracking-[0.18em] text-stone-400">{selectedNode.data.kind}</div>
          </div>
          <label className="block space-y-2">
            <span className="text-xs uppercase tracking-[0.18em] text-stone-400">Title</span>
            <input
              className="w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-teal"
              onChange={(event) => setTitle(event.target.value)}
              value={title}
            />
          </label>
          <label className="block space-y-2">
            <span className="text-xs uppercase tracking-[0.18em] text-stone-400">Content</span>
            <textarea
              className="min-h-32 w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm leading-6 outline-none transition focus:border-teal"
              onChange={(event) => setContent(event.target.value)}
              value={content}
            />
          </label>
          <label className="block space-y-2">
            <span className="text-xs uppercase tracking-[0.18em] text-stone-400">
              {selectedNode.data.kind === "image" ? "Provider or model" : "Model"}
            </span>
            <input
              className="w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-teal"
              onChange={(event) => setModel(event.target.value)}
              placeholder={selectedNode.data.kind === "image" ? "Optional local checkpoint/model name" : "Optional model tag"}
              value={model}
            />
          </label>
          {selectedNode.data.kind === "image" ? (
            <button
              className="w-full rounded-2xl bg-coral px-4 py-3 text-sm font-semibold text-white transition hover:bg-coral/90 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={generatingNodeId === selectedNode.id}
              onClick={async () => {
                await updateSelectedNode({ title, content, model });
                await generateSelectedNode();
              }}
              type="button"
            >
              {generatingNodeId === selectedNode.id ? "Generating..." : "Generate image"}
            </button>
          ) : null}
          {parsedOutput?.kind === "image" ? (
            <div className="overflow-hidden rounded-2xl border border-teal/20 bg-white">
              <img alt={selectedNode.data.title} className="w-full object-cover" src={parsedOutput.src} />
            </div>
          ) : null}
          {parsedOutput?.kind === "text" ? (
            <div className="rounded-2xl border border-teal/20 bg-teal/5 p-3 text-sm leading-6 text-stone-700">
              {parsedOutput.raw}
            </div>
          ) : null}
          <button
            className="w-full rounded-2xl bg-slateblue px-4 py-3 text-sm font-semibold text-white transition hover:bg-slateblue/90 disabled:cursor-not-allowed disabled:opacity-60"
            disabled={savingNodeId === selectedNode.id}
            type="submit"
          >
            {savingNodeId === selectedNode.id ? "Saving..." : "Save node"}
          </button>
          <button
            className="mt-2 w-full rounded-2xl bg-red-600 px-4 py-3 text-sm font-semibold text-white transition hover:bg-red-700"
            type="button"
            onClick={async () => {
              if (!selectedNode) return;
              await useBoardStore.getState().deleteNode(selectedNode.id);
            }}
          >
            Delete node
          </button>
        </form>
      ) : (
        <div className="mt-3 rounded-2xl border border-dashed border-stone-300 p-4 text-sm leading-6 text-stone-500">
          Select a node to inspect its content, status, and generated output.
        </div>
      )}
    </section>
  );
}
