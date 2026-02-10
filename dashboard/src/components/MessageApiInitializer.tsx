"use client";

import { App } from "antd";
import { useEffect } from "react";
import { setMessageApi } from "#/utils/messageHolder";

const MessageApiInitializer = ({ children }: { children: React.ReactNode }) => {
  const { message } = App.useApp();

  useEffect(() => {
    setMessageApi(message);
  }, [message]);

  return <>{children}</>;
};

export default MessageApiInitializer;
