import { HubConnectionBuilder, HubConnection, LogLevel, HubConnectionState } from "@microsoft/signalr";

let connection: HubConnection | null = null;

export async function ensureConnection(hubUrl: string) {
  if (connection && connection.state === HubConnectionState.Connected) return connection;
  console.debug("[signalr] ensuring connection to", hubUrl);
  connection = new HubConnectionBuilder()
    .withUrl(hubUrl)
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  connection.onclose((err) => {
    console.warn("[signalr] connection closed", err);
  });

  try {
    await connection.start();
    console.info("[signalr] connected", hubUrl);
    return connection;
  } catch (err) {
    console.error("[signalr] failed to start connection", err);
    throw err;
  }
}

export async function joinBoardGroup(hubUrl: string, boardId: string) {
  const conn = await ensureConnection(hubUrl);
  try {
    await conn.invoke("JoinBoard", boardId);
    console.debug("[signalr] joined board group", boardId);
  } catch (err) {
    console.error("[signalr] failed to join board group", err);
    throw err;
  }
}

export function onNodeUpdated(callback: (payload: any) => void) {
  if (!connection) return () => {};
  connection.on("NodeUpdated", callback);
  return () => connection?.off("NodeUpdated", callback);
}
