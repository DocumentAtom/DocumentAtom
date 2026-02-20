import { Metadata } from "next";
import LoginPage from "#/page/login/LoginPage";

export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "Document Atom | Home",
  description: "Document Atom | Home",
};

export default function Home() {
  const serverUrl =
    process.env.DOCUMENTATOM_SERVER_URL || "http://localhost:3000";
  return <LoginPage defaultServerUrl={serverUrl} />;
}
