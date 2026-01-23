import React from "react";
import styles from "./loginLayout.module.scss";
import DocuAtomFlex from "../base/flex/Flex";
import DocuAtomLogo from "../logo/Logo";
import DocuAtomText from "../base/typograpghy/Text";
import { DocumentAtomTheme } from "#/theme/theme";
import ThemeModeSwitch from "../theme-mode-switch/ThemeModeSwitch";

const LoginLayout = ({ children }: { children: React.ReactNode }) => {
  return (
    <DocuAtomFlex
      className={styles.loginLayout}
      vertical
      justify="space-between"
    >
      <DocuAtomFlex
        className={styles.header}
        justify="space-between"
        align="center"
      >
        <DocuAtomLogo />
        <ThemeModeSwitch />
      </DocuAtomFlex>
      <div className={styles.content}>{children}</div>
      <DocuAtomFlex className={styles.footer} justify="center" align="center">
        <DocuAtomText color={DocumentAtomTheme.borderGray}></DocuAtomText>
      </DocuAtomFlex>
    </DocuAtomFlex>
  );
};

export default LoginLayout;
