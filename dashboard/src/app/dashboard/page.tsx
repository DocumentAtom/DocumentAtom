import { Metadata } from "next";
import React from "react";
import HomePage from "#/page/home-page/HomePage";

export const metadata: Metadata = {
  title: "Home | Document Atom",
  description: "Home | Document Atom",
};

const Page = () => {
  return <HomePage />;
};

export default Page;
