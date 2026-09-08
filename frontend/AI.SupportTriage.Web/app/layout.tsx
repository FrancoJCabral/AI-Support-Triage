import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "AI Support Triage",
  description:
    "Clasificación inteligente de consultas de soporte mediante IA aplicada a operaciones.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="es">
      <body>{children}</body>
    </html>
  );
}
