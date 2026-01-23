import { Metadata } from "next";
import LoginPage from "#/page/login/LoginPage";

export const metadata: Metadata = {
  title: "Document Atom | Home",
  description: "Document Atom | Home",
};

export default function Home() {
  return <LoginPage />;
}
