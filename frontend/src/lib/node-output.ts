export type ParsedNodeOutput =
  | {
      kind: "image";
      src: string;
      mimeType: string;
      revisedPrompt?: string;
      raw: string;
    }
  | {
      kind: "text";
      raw: string;
    };

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080/api/v1";
const API_ORIGIN = API_BASE_URL.replace(/\/api\/v1\/?$/, "");

function resolveImageUrl(imageUrl: string) {
  if (/^https?:\/\//i.test(imageUrl)) {
    return imageUrl;
  }

  // strip any api version prefix like /api/v1 or /api/v2 if present
  const cleaned = imageUrl.replace(/^\/api\/v\d+/, "");
  if (cleaned.startsWith("/")) {
    return `${API_ORIGIN}${cleaned}`;
  }

  return `${API_ORIGIN}/${cleaned}`;
}

export function parseNodeOutput(outputContent?: string | null): ParsedNodeOutput | null {
  if (!outputContent) {
    return null;
  }

  try {
    const parsed = JSON.parse(outputContent) as {
      type?: string;
      imageBase64?: string;
      imageUrl?: string;
      mimeType?: string;
      revisedPrompt?: string;
      content?: string;
    };

    if (parsed.type === "image" && parsed.imageUrl) {
      return {
        kind: "image",
        src: resolveImageUrl(parsed.imageUrl),
        mimeType: parsed.mimeType || "image/png",
        revisedPrompt: parsed.revisedPrompt,
        raw: outputContent,
      };
    }

    if (parsed.type === "image" && parsed.imageBase64) {
      const mimeType = parsed.mimeType || "image/png";
      return {
        kind: "image",
        src: `data:${mimeType};base64,${parsed.imageBase64}`,
        mimeType,
        revisedPrompt: parsed.revisedPrompt,
        raw: outputContent,
      };
    }

    if (typeof parsed.content === "string" && parsed.content.trim()) {
      return { kind: "text", raw: parsed.content };
    }
  } catch {
  }

  return { kind: "text", raw: outputContent };
}
