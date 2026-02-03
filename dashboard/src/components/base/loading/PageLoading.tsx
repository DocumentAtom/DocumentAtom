"use client";
import React from "react";
import { LoadingOutlined } from "@ant-design/icons";
import styles from "./pageLoding.module.scss";
import DocuAtomText from "../typograpghy/Text";
import DocuAtomFlex from "../flex/Flex";
import classNames from "classnames";

const PageLoading = ({
  message = "Loading...",
  dataTestId,
}: {
  message?: string | React.ReactNode;
  dataTestId?: string;
}) => {
  return (
    <DocuAtomFlex
      data-testid={dataTestId}
      className={classNames(styles.pageLoading, "mt")}
      justify="center"
      align="center"
      vertical
    >
      <DocuAtomText>{message}</DocuAtomText>
      <LoadingOutlined className={styles.pageLoader} />
    </DocuAtomFlex>
  );
};

export default PageLoading;
