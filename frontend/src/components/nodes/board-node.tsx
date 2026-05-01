"use client";

import { Handle, Position, type Node, type NodeProps } from "@xyflow/react";
import clsx from "clsx";
import { parseNodeOutput } from "@/lib/node-output";

type Data = {
  title: string;
  content: string;
  outputContent?: string | null;
  kind: "text" | "image" | "prompt" | "assistant" | "group";
  status: number;
};

const accentByKind: Record<Data["kind"], string> = {
  text: "border-slateblue/30 bg-white/90",
  image: "border-coral/40 bg-coral/10",
  prompt: "border-mustard/70 bg-mustard/25",
  assistant: "border-teal/45 bg-teal/10",
  group: "border-ink/25 bg-stone-200/80",
};

type BoardCanvasNode = Node<Data>;

export function BoardNode({ data, selected }: NodeProps<BoardCanvasNode>) {
  const parsedOutput = parseNodeOutput(data.outputContent);

  return (
    <div
      className={clsx(
        "min-w-[220px] max-w-[280px] rounded-2xl border px-4 py-3 shadow-card backdrop-blur-sm transition",
        accentByKind[data.kind],
        selected && "ring-2 ring-teal/60",
      )}
    >
      <Handle type="target" position={Position.Left} className="!h-3 !w-3 !border-0 !bg-slateblue" />
      <div className="mb-2 flex items-center justify-between gap-3">
        <div>
             <div className="text-xs uppercase tracking-[0.24em] text-stone-500">{data.kind}</div>
             <div className="text-sm font-semibold text-stone-800">{data.title}</div>
        </div>
        <div className="rounded-full bg-white/80 px-2 py-1 text-[10px] font-semibold uppercase tracking-[0.18em] text-stone-500">
          {statusLabel(data.status)}
        </div>
      </div>
      <div className="line-clamp-4 text-sm leading-6 text-stone-700">{data.content}</div>
      {parsedOutput?.kind === "image" ? (
        <div className="mt-3 relative overflow-hidden rounded-xl border border-teal/20 bg-white/75">
          <img alt={data.title} className="h-36 w-full object-cover" src={parsedOutput.src} />
          {(data.status === 2 || data.status === 3) ? (
            <div className="absolute inset-0 flex items-center justify-center bg-black/40">
              <div className="rounded-full bg-white/10 px-3 py-2 text-sm font-semibold text-white">Processing...</div>
            </div>
          ) : null}
        </div>
      ) : null}
      {parsedOutput?.kind === "text" ? (
         <div className="mt-3 rounded-xl border border-teal/20 bg-white/75 p-3 text-xs leading-5 text-stone-800">
          {parsedOutput.raw}
        </div>
      ) : null}
      <Handle type="source" position={Position.Right} className="!h-3 !w-3 !border-0 !bg-coral" />
    </div>
  );
}

function statusLabel(status: number) {
  switch (status) {
    case 2:
      return "Queued";
    case 3:
      return "Running";
    case 4:
      return "Done";
    case 5:
      return "Failed";
    default:
      return "Idle";
  }
}
