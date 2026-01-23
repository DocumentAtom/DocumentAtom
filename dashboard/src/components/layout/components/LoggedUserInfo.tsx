import React from "react";
import {
  DownCircleOutlined,
  AccountBookOutlined,
  LogoutOutlined,
  UserOutlined,
} from "@ant-design/icons";
import DocuAtomSpace from "#/components/base/space/Space";
import DocuAtomText from "#/components/base/typograpghy/Text";
import { getFirstLetterOfTheWord } from "#/utils/stringUtils";

import DocuAtomDropdown from "#/components/base/dropdown/Dropdown";
import DocuAtomAvatar from "#/components/base/avatar/Avatar";
import { DocumentAtomTheme } from "#/theme/theme";
import { MenuProps } from "antd";
import { usePathname } from "next/navigation";
import { getDashboardPathKey } from "#/utils/appUtils";
import styles from "./styles.module.scss";

const items: MenuProps["items"] = [
  {
    label: "Profile",
    key: "profile",
    icon: <UserOutlined />,
  },
  {
    label: "Account",
    key: "account",
    icon: <AccountBookOutlined />,
  },
  {
    label: "Logout",
    key: "logout",
    icon: <LogoutOutlined />,
  },
];

const LoggedUserInfo = () => {
  const pathname = usePathname();

  const userName = "User";

  const onClick: MenuProps["onClick"] = ({ key }: { key: string }) => {};

  return (
    <DocuAtomDropdown menu={{ items, onClick }} trigger={["click"]}>
      <DocuAtomSpace className={styles.container}>
        <DocuAtomText className="ant-color-white" strong weight={400}>
          {userName}
        </DocuAtomText>
        <DocuAtomAvatar
          alt="User Profile"
          src={!userName && "/profile-pic.png"}
          size={"small"}
          style={{ background: DocumentAtomTheme.primary }}
        >
          {getFirstLetterOfTheWord(userName)}
        </DocuAtomAvatar>
        <DownCircleOutlined className="ant-color-white" />
      </DocuAtomSpace>
    </DocuAtomDropdown>
  );
};

export default LoggedUserInfo;
