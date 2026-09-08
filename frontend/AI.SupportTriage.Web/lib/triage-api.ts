import type { TriageRequest, TriageResult } from "@/types/triage";

interface ProblemDetails {
  title?: string;
  detail?: string;
}

const fallbackError =
  "No pudimos analizar la consulta. Verificá la conexión e intentá nuevamente.";

export async function analyzeSupportRequest(
  request: TriageRequest,
): Promise<TriageResult> {
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, "");

  if (!baseUrl) {
    throw new Error(
      "La URL del servicio no está configurada. Revisá NEXT_PUBLIC_API_BASE_URL.",
    );
  }

  let response: Response;

  try {
    response = await fetch(`${baseUrl}/api/triage`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    });
  } catch {
    throw new Error(fallbackError);
  }

  if (!response.ok) {
    const problem = await readProblemDetails(response);
    throw new Error(problem.detail ?? problem.title ?? fallbackError);
  }

  return (await response.json()) as TriageResult;
}

async function readProblemDetails(response: Response): Promise<ProblemDetails> {
  try {
    const value: unknown = await response.json();

    if (typeof value === "object" && value !== null) {
      const record = value as Record<string, unknown>;

      return {
        title: typeof record.title === "string" ? record.title : undefined,
        detail: typeof record.detail === "string" ? record.detail : undefined,
      };
    }
  } catch {
    // A non-JSON error response falls back to the safe generic message.
  }

  return {};
}
