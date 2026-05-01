"use client";

import { useState } from "react";
import type { ApiBoard } from "@/lib/types";
import { Bot, BrainCircuit, ImagePlus, MessageSquarePlus, ScanSearch, StickyNote } from "lucide-react";
import { BoardCanvas } from "@/components/board-canvas";
import { InspectorPanel } from "@/components/panels/inspector-panel";
import { AssistantPanel } from "@/components/panels/assistant-panel";
import { useBoardStore } from "@/store/board-store";
import type { NodeKind } from "@/lib/types";

const quickActions: { kind: NodeKind; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { kind: "text", label: "Text node", icon: StickyNote },
  { kind: "prompt", label: "Prompt node", icon: MessageSquarePlus },
  { kind: "image", label: "Image node", icon: ImagePlus },
  { kind: "assistant", label: "Assistant node", icon: Bot },
];

export function BoardShell() {
  const {
    boardId,
    boardName,
    addNode,
    createNewBoard,
    error,
    loadBoard,
    loading,
  } = useBoardStore();

  const [boardNameInput, setBoardNameInput] = useState("Creative campaign board");
  const [boardIdInput, setBoardIdInput] = useState("");
  const [showLeft, setShowLeft] = useState(true);
  const [showRight, setShowRight] = useState(true);
  const [boards, setBoards] = useState<ApiBoard[]>([]);

  return (
    <main className="min-h-screen h-screen overflow-hidden px-4 py-5 md:px-6">
      <div className="mx-auto max-w-[1720px] gap-4 relative">
        <div className="col-span-full flex items-center justify-between">
          <div className="flex gap-2">
            <button
              className="rounded-full bg-white/10 p-2 text-sm"
              onClick={() => setShowLeft((v) => !v)}
              type="button"
            >
              {showLeft ? "Hide left" : "Show left"}
            </button>
            <button
              className="rounded-full bg-white/10 p-2 text-sm"
              onClick={() => setShowRight((v) => !v)}
              type="button"
            >
              {showRight ? "Hide right" : "Show right"}
            </button>
          </div>
        </div>
        <aside className={`fixed left-6 top-20 z-30 h-[calc(100vh-80px)] w-72 max-h-[calc(100vh-80px)] overflow-y-auto overflow-x-hidden rounded-[28px] transition-transform shadow-card ${showLeft ? "translate-x-0" : "-translate-x-[110%]"}`}>
          <section className="rounded-[28px] border border-white/70 bg-white/75 p-5 shadow-card backdrop-blur-sm">
            <div className="text-xs uppercase tracking-[0.28em] text-stone-400">AI Workflow Board</div>
            <h1 className="mt-3 text-3xl font-semibold tracking-tight text-ink">Infinite board for prompts, images, and assistants</h1>
            <p className="mt-3 text-sm leading-6 text-stone-400">
              React Flow handles the graph editor, while the backend indexes node content into semantic memory for RAG.
            </p>
          </section>

          <section className="rounded-[28px] border border-white/70 bg-white/75 p-5 shadow-card backdrop-blur-sm">
            <div className="flex items-center gap-2 text-xs uppercase tracking-[0.24em] text-stone-400">
              <BrainCircuit className="h-4 w-4" />
              Board controls
            </div>

            <div className="mt-4 space-y-3">
              <input
                className="w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-teal"
                onChange={(event) => setBoardNameInput(event.target.value)}
                placeholder="Board name"
                value={boardNameInput}
              />
              <button
                className="w-full rounded-2xl bg-slateblue px-4 py-3 text-sm font-semibold text-white transition hover:bg-slateblue/90"
                onClick={() => createNewBoard(boardNameInput)}
                type="button"
              >
                Create board
              </button>
            </div>

            <div className="mt-4 border-t border-stone-200 pt-4">
              <input
                className="w-full rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-teal"
                onChange={(event) => setBoardIdInput(event.target.value)}
                placeholder="Paste a board id"
                value={boardIdInput}
              />
              <button
                className="mt-3 w-full rounded-2xl border border-stone-300 px-4 py-3 text-sm font-semibold text-gray-300 transition hover:bg-stone-100 hover:text-stone-500"
                onClick={() => loadBoard(boardIdInput)}
                type="button"
              >
                Load board
              </button>
            </div>

            <div className="mt-4 rounded-2xl bg-sand/80 p-4 text-sm leading-6 text-stone-700">
              <div className="font-semibold text-ink">{boardName}</div>
              <div className="mt-1 break-all text-xs uppercase tracking-[0.16em] text-stone-500">
                {boardId ?? "No board selected"}
              </div>
            </div>

            {error ? (
              <div className="mt-4 rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                {error}
              </div>
            ) : null}
          </section>

          <section className="rounded-[28px] border border-white/70 bg-white/75 p-5 shadow-card backdrop-blur-sm">
            <div className="flex items-center gap-2 text-xs uppercase tracking-[0.24em] text-stone-400">
              <ScanSearch className="h-4 w-4" />
              Quick nodes
            </div>
            <div className="mt-4 space-y-2">
              {quickActions.map((action, index) => {
                const Icon = action.icon;
                return (
                  <button
                    key={action.kind}
                    className="flex w-full items-center justify-between rounded-2xl border border-stone-200 bg-white/80 px-4 py-3 text-left text-sm text-stone-800 transition hover:-translate-y-0.5 hover:border-stone-300"
                    onClick={() => addNode(action.kind, { x: 120 + index * 30, y: 120 + index * 30 })}
                    type="button"
                  >
                    <span className="inline-flex items-center gap-3">
                      <Icon className="h-4 w-4 text-teal" />
                      {action.label}
                    </span>
                    <span className="text-xs uppercase tracking-[0.18em] text-stone-400">add</span>
                  </button>
                );
              })}
            </div>
          </section>
        </aside>

        <section className="min-w-0 h-[calc(100vh-50px)] pb-8 overflow-hidden">
          <BoardCanvas />
        </section>

        <aside className={`fixed right-6 top-20 z-30 h-[calc(100vh-80px)] w-[360px] max-h-[calc(100vh-80px)] overflow-y-auto overflow-x-hidden rounded-[20px] transition-transform shadow-card ${showRight ? "translate-x-0" : "translate-x-[110%]"}`}>
          <div className="p-4 space-y-3">
            <InspectorPanel />
            <AssistantPanel />

            <section className="rounded-[24px] border border-white/70 bg-white/75 p-4 shadow-card backdrop-blur-sm">
              <div className="text-xs uppercase tracking-[0.24em] text-stone-400">Your boards</div>
              <div className="mt-3 space-y-2">
                <button
                  className="w-full rounded-2xl border border-stone-200 bg-white/5 px-3 py-2 text-sm text-stone-400"
                  onClick={async () => setBoards(await useBoardStore.getState().listBoards())}
                  type="button"
                >
                  Refresh boards
                </button>

                <div className="mt-2 space-y-2">
                  {boards.map((b) => (
                    <div key={b.id} className="flex items-center justify-between rounded-lg border border-white/10 p-2">
                      <div className="text-sm text-stone-300">{b.name}</div>
                      <button
                        className="text-xs text-teal font-bold"
                        onClick={() => loadBoard(b.id)}
                        type="button"
                      >
                        Open
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </section>
          </div>
        </aside>
      </div>
    </main>
  );
}
