"use client";

import { FormEvent, useState } from "react";
import { analyzeSupportRequest } from "@/lib/triage-api";
import type { TriageResult } from "@/types/triage";

const maxLength = 4000;

const examples = [
  "No puedo completar el pago con Visa y necesito resolverlo hoy.",
  "I forgot my password and cannot sign in.",
  "My package has not arrived and tracking hasn't updated.",
];

type RequestState = "idle" | "loading" | "success" | "error";

export function TriageWorkspace() {
  const [message, setMessage] = useState("");
  const [status, setStatus] = useState<RequestState>("idle");
  const [result, setResult] = useState<TriageResult | null>(null);
  const [error, setError] = useState("");

  const isWhitespaceOnly = message.length > 0 && message.trim().length === 0;
  const canSubmit = message.trim().length > 0 && status !== "loading";

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const normalizedMessage = message.trim();
    if (!normalizedMessage) return;

    setStatus("loading");
    setError("");

    try {
      const triageResult = await analyzeSupportRequest({
        message: normalizedMessage,
      });
      setResult(triageResult);
      setStatus("success");
    } catch (requestError: unknown) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Ocurrió un error inesperado. Intentá nuevamente.",
      );
      setStatus("error");
    }
  }

  function loadExample(example: string) {
    setMessage(example);
    setError("");
    if (status === "error") setStatus("idle");
  }

  return (
    <div className="workspace">
      <section className="input-card" aria-labelledby="input-heading">
        <div className="section-heading">
          <span className="step-number">01</span>
          <div>
            <p className="eyebrow">Nueva consulta</p>
            <h2 id="input-heading">¿Cómo podemos ayudar?</h2>
          </div>
        </div>

        <form onSubmit={handleSubmit} noValidate>
          <div className="field-header">
            <label htmlFor="support-message">Mensaje de soporte</label>
            <span className="character-count" aria-live="polite">
              {message.length.toLocaleString("es-AR")} / {maxLength.toLocaleString("es-AR")}
            </span>
          </div>
          <textarea
            id="support-message"
            name="message"
            value={message}
            maxLength={maxLength}
            rows={9}
            placeholder="No puedo completar el pago con Visa y necesito resolverlo hoy."
            aria-describedby="message-help message-error"
            aria-invalid={isWhitespaceOnly}
            onChange={(event) => setMessage(event.target.value)}
          />
          <div className="field-footer">
            <p id="message-help">Contanos qué ocurrió con el mayor contexto posible.</p>
            {isWhitespaceOnly && (
              <p id="message-error" className="validation-message">
                Ingresá un mensaje válido para continuar.
              </p>
            )}
          </div>

          <div className="examples" aria-label="Ejemplos rápidos">
            <p>Probá con un ejemplo</p>
            <div className="example-list">
              {examples.map((example, index) => (
                <button
                  key={example}
                  type="button"
                  className="example-chip"
                  onClick={() => loadExample(example)}
                >
                  <span>0{index + 1}</span>
                  {index === 0 ? "Pago urgente" : index === 1 ? "Acceso a cuenta" : "Seguimiento de envío"}
                </button>
              ))}
            </div>
          </div>

          <button className="submit-button" type="submit" disabled={!canSubmit}>
            {status === "loading" ? (
              <>
                <span className="spinner" aria-hidden="true" />
                Analizando...
              </>
            ) : (
              <>
                Analizar consulta
                <span aria-hidden="true">→</span>
              </>
            )}
          </button>
        </form>
      </section>

      <section className="result-card" aria-labelledby="result-heading" aria-live="polite">
        <div className="section-heading">
          <span className="step-number">02</span>
          <div>
            <p className="eyebrow">Resultado del análisis</p>
            <h2 id="result-heading">Ruta recomendada</h2>
          </div>
        </div>

        {status === "success" && result ? (
          <TriageResultView result={result} />
        ) : status === "error" ? (
          <div className="state-panel error-state" role="alert">
            <span className="state-icon" aria-hidden="true">!</span>
            <h3>No se pudo completar el análisis</h3>
            <p>{error}</p>
            <button type="button" onClick={() => setStatus("idle")}>
              Volver a intentar
            </button>
          </div>
        ) : status === "loading" ? (
          <div className="state-panel loading-state">
            <div className="analysis-pulse" aria-hidden="true">
              <span />
              <span />
              <span />
            </div>
            <h3>Analizando la consulta</h3>
            <p>Estamos evaluando contexto, prioridad y sentimiento.</p>
          </div>
        ) : (
          <div className="state-panel empty-state">
            <div className="empty-visual" aria-hidden="true">
              <span className="empty-line" />
              <span className="empty-line short" />
              <span className="empty-dot one" />
              <span className="empty-dot two" />
              <span className="empty-dot three" />
            </div>
            <h3>Tu análisis aparecerá acá</h3>
            <p>Ingresá una consulta para conocer su clasificación y el equipo sugerido.</p>
          </div>
        )}
      </section>
    </div>
  );
}

function TriageResultView({ result }: { result: TriageResult }) {
  return (
    <div className="result-content">
      <div className="result-status">
        <span className="status-dot" aria-hidden="true" />
        Análisis completado
      </div>

      <dl className="metrics-grid">
        <div>
          <dt>Categoría</dt>
          <dd><span className="badge category">{result.category}</span></dd>
        </div>
        <div>
          <dt>Prioridad</dt>
          <dd><span className="badge" data-priority={result.priority}>{result.priority}</span></dd>
        </div>
        <div>
          <dt>Sentimiento</dt>
          <dd><span className="badge" data-sentiment={result.sentiment}>{result.sentiment}</span></dd>
        </div>
      </dl>

      <div className="team-block">
        <p>Equipo sugerido</p>
        <div>
          <span className="team-mark" aria-hidden="true">→</span>
          <strong>{result.suggestedTeam}</strong>
        </div>
      </div>

      <div className="summary-block">
        <p>Resumen</p>
        <blockquote>{result.summary}</blockquote>
      </div>
    </div>
  );
}
