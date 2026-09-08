import { TriageWorkspace } from "@/components/triage-workspace";

export default function Home() {
  return (
    <main>
      <header className="topbar">
        <a href="#main-content" className="brand" aria-label="AI Support Triage, inicio">
          <span className="brand-mark" aria-hidden="true"><i /><i /><i /></span>
          <span>AI Support Triage</span>
        </a>
        <div className="availability"><span aria-hidden="true" /> Sistema disponible</div>
      </header>

      <div id="main-content" className="page-shell">
        <section className="hero" aria-labelledby="page-title">
          <div>
            <p className="hero-kicker"><span /> Inteligencia aplicada a soporte</p>
            <h1 id="page-title">Cada consulta, en la ruta correcta.</h1>
          </div>
          <p className="hero-copy">
            Clasificación inteligente de consultas de soporte y derivación automática.
            Detectá categoría, prioridad y sentimiento en segundos.
          </p>
        </section>

        <TriageWorkspace />

        <footer>
          <p>AI Support Triage</p>
          <span>Clasificación · Priorización · Derivación</span>
        </footer>
      </div>
    </main>
  );
}
