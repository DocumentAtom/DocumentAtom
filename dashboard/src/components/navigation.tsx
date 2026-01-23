"use client";
import { Button, Layout } from "antd";
import DocuAtomFlex from "./base/flex/Flex";
import MenuItems from "./menu-item/MenuItems";
import { MenuItemProps } from "./menu-item/types";
import { DoubleLeftOutlined, DoubleRightOutlined } from "@ant-design/icons";
import styles from "./layout/nav.module.scss";
import DocuAtomLogo from "./logo/Logo";

const { Sider } = Layout;

const Navigation = ({
  collapsed,
  menuItems,
  setCollapsed,
}: {
  collapsed: boolean;
  menuItems: MenuItemProps[];
  setCollapsed: (collapsed: boolean) => void;
}) => {
  return (
    <Sider
      theme="light"
      width={170}
      trigger={null}
      collapsible
      collapsed={collapsed}
      collapsedWidth={60}
      className={styles.sidebarContainer}
    >
      <DocuAtomFlex
        justify="center"
        gap={8}
        align="center"
        className={styles.logoContainer}
      >
        <DocuAtomLogo showOnlyIcon={true} />
      </DocuAtomFlex>
      <DocuAtomFlex justify="flex-end" className="pl-sm pr-sm pt-sm">
        <Button
          type="link"
          icon={collapsed ? <DoubleRightOutlined /> : <DoubleLeftOutlined />}
          onClick={() => setCollapsed(!collapsed)}
          style={{
            fontSize: "16px",
            top: "150",
            right: "8%",
          }}
        />
      </DocuAtomFlex>
      <MenuItems menuItems={menuItems} collapsed={collapsed} />
    </Sider>
  );
};

export default Navigation;
