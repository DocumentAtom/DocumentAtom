"use client";
import { useRef } from "react";
import { Provider } from "react-redux";
import { makeStore, AppStore } from "./store";

export default function StoreProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  // Lazily create one store per client, as recommended in the Redux Toolkit
  // Next.js guide; reading the ref during render is intentional here.
  /* eslint-disable react-hooks/refs */
  const storeRef = useRef<AppStore | null>(null);
  if (!storeRef.current) {
    // Create the store instance the first time this renders
    storeRef.current = makeStore();
  }

  return <Provider store={storeRef.current}>{children}</Provider>;
  /* eslint-enable react-hooks/refs */
}
