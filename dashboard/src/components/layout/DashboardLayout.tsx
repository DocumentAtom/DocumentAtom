"use client";

import React, { useEffect, useState } from "react";
import { Button, Layout, Tooltip, theme } from "antd";
import {
  GithubOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import styles from "./dashboard.module.scss";
import DocuAtomFlex from "../base/flex/Flex";
import ErrorBoundary from "#/hoc/ErrorBoundary";
import ThemeModeSwitch from "../theme-mode-switch/ThemeModeSwitch";
import Link from "next/link";
import { localStorageKeys, paths } from "#/constants/constant";
import DocuAtomLogo from "../logo/Logo";
import { apiEndpointURL } from "#/constants/config";

const { Header, Content } = Layout;

interface LayoutWrapperProps {
  children: React.ReactNode;
}

const DashboardLayout = ({ children }: LayoutWrapperProps) => {
  const { token } = theme.useToken();
  const [serverUrl, setServerUrl] = useState(apiEndpointURL);

  useEffect(() => {
    const savedUrl = localStorage.getItem(localStorageKeys.documentAtomAPIUrl);
    setServerUrl(savedUrl || apiEndpointURL);
  }, []);

  return (
    <Layout style={{ minHeight: "100vh" }}>
      <Layout>
        <Header className={styles.header}>
          <DocuAtomFlex align="center" gap={10}>
            <DocuAtomLogo size={16} imageSize={35} text="DocumnetAtom" />
          </DocuAtomFlex>

          <span className={styles.serverUrl}>{serverUrl}</span>

          <DocuAtomFlex gap={10} align="center" className={styles.headerActions}>
            <Tooltip title="View on GitHub">
              <Button
                aria-label="View on GitHub"
                icon={<GithubOutlined />}
                type="text"
                href="https://github.com/documentatom/documentatom"
                target="_blank"
                rel="noopener noreferrer"
              />
            </Tooltip>
            <ThemeModeSwitch />
            <Link href={paths.login} aria-label="Log out">
              <Tooltip title="Log out">
                <Button
                  aria-label="Log out"
                  icon={<LogoutOutlined />}
                  type="text"
                />
              </Tooltip>
            </Link>
          </DocuAtomFlex>
        </Header>
        <Content
          style={{
            minHeight: 280,
            background: token.colorBgContainer,
          }}
        >
          <ErrorBoundary>{children}</ErrorBoundary>
        </Content>
      </Layout>
    </Layout>
  );
};

export default DashboardLayout;
