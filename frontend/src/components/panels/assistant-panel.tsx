"use client";

import { useState } from "react";
import { SendHorizonal } from "lucide-react";
import { useBoardStore } from "@/store/board-store";

export function AssistantPanel() {
  const { chat, sendAssistantMessage, boardId, loading } = useBoardStore();
  const [message, setMessage] = useState("");

  return (
    <section className="flex h-[360px] flex-col rounded-[24px] border border-white/70 bg-white/75 p-4 shadow-card backdrop-blur-sm">
      <div className="mb-3">
        <div className="text-xs uppercase tracking-[0.24em] text-stone-400">AI panel</div>
        <div className="mt-1 text-lg font-semibold text-ink">Board assistant</div>
        <div className="mt-1 text-sm text-stone-500">
          {boardId ? "Grounded on retrieved board memory." : "Create or load a board to use RAG context."}
        </div>
      </div>

      <div className="flex-1 space-y-3 overflow-y-auto rounded-2xl bg-sand/70 p-3">
        {chat.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-stone-300 bg-white/60 p-4 text-sm leading-6 text-stone-500">
            Ask for next prompts, research summaries, or how to connect nodes. The backend assistant will retrieve semantic memory from the board first.
          </div>
        ) : null}
        {chat.map((item) => (
          <div
            key={item.id}
            className={item.role === "user"
              ? "ml-8 rounded-2xl bg-slateblue px-4 py-3 text-sm text-white"
              : "mr-8 rounded-2xl bg-white px-4 py-3 text-sm text-stone-700"}
          >
            {item.content}
          </div>
        ))}
      </div>

      <form
        className="mt-3 flex gap-2"
        onSubmit={async (event) => {
          event.preventDefault();
          const trimmed = message.trim();
          if (!trimmed || loading) {
            return;
          }

          setMessage("");
          await sendAssistantMessage(trimmed);
        }}
      >
        <input
          className="flex-1 rounded-2xl border border-stone-200 bg-white px-4 py-3 text-sm outline-none transition focus:border-teal"
          onChange={(event) => setMessage(event.target.value)}
          placeholder="Ask the assistant about this board..."
          value={message}
        />
        <button
          className="inline-flex items-center gap-2 rounded-2xl bg-teal px-4 py-3 text-sm font-semibold text-white transition hover:bg-teal/90"
          type="submit"
        >
          <SendHorizonal className="h-4 w-4" />
          Send
        </button>
      </form>
    </section>
  );
}
