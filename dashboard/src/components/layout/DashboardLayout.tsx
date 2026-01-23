import React, { useState } from "react";
import { Layout } from "antd";
import styles from "./dashboard.module.scss";
import DocuAtomFlex from "../base/flex/Flex";
import ErrorBoundary from "#/hoc/ErrorBoundary";
import ThemeModeSwitch from "../theme-mode-switch/ThemeModeSwitch";
import Link from "next/link";
import { paths } from "#/constants/constant";
import DocuAtomLogo from "../logo/Logo";

const { Header, Content } = Layout;

interface LayoutWrapperProps {
  children: React.ReactNode;
}

const DashboardLayout = ({ children }: LayoutWrapperProps) => {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <Layout style={{ minHeight: "100vh" }}>
      {/* <Navigation
        collapsed={collapsed}
        menuItems={dashboardRoutes}
        setCollapsed={setCollapsed}
      /> */}
      <Layout>
        <Header className={styles.header}>
          <DocuAtomFlex align="center" gap={10}>
            <DocuAtomLogo size={16} imageSize={35} />
            <ThemeModeSwitch />
          </DocuAtomFlex>

          <DocuAtomFlex gap={10} align="center">
            <Link href={paths.login}>
              <b>Change Server URL</b>
            </Link>
          </DocuAtomFlex>
        </Header>
        <Content
          style={{
            minHeight: 280,
            background: "#fff",
          }}
        >
          <ErrorBoundary>{children}</ErrorBoundary>
        </Content>
      </Layout>
    </Layout>
  );
};

export default DashboardLayout;
