import React from "react";
import classNames from "classnames";
import DocuAtomFlex from "../flex/Flex";
import DocuAtomText from "../typograpghy/Text";
import styles from "./pageContainer.module.scss";
import { Content } from "antd/es/layout/layout";
import { ArrowLeftOutlined } from "@ant-design/icons";
import Link from "next/link";
import { primaryTheme } from "#/theme/theme";

const PageContainer = ({
  children,
  className,
  withoutWhiteBG = false,
  id,
  pageTitle,
  pageTitleRightContent,
  backPath,
  dataTestId,
  is100vh = false,
  style,
  pageTitleClassName,
}: {
  children: React.ReactNode;
  className?: string;
  withoutWhiteBG?: boolean;
  id?: string;
  pageTitle?: React.ReactNode;
  pageTitleRightContent?: React.ReactNode;
  showGraphSelector?: boolean;
  backPath?: string;
  dataTestId?: string;
  is100vh?: boolean;
  style?: React.CSSProperties;
  pageTitleClassName?: string;
}) => {
  return (
    <Content
      className={classNames(className, !withoutWhiteBG && styles.pageContainer)}
      id={id}
      data-testid={dataTestId}
      style={{ height: is100vh ? "100vh" : "auto", ...style }}
    >
      {(pageTitle || pageTitleRightContent) && (
        <>
          <DocuAtomFlex
            className={classNames(
              styles.pageTitleContainer,
              pageTitleClassName
            )}
            wrap
            gap="small"
            align="center"
            justify="space-between"
          >
            <DocuAtomFlex align="center" gap="small">
              {backPath && (
                <Link href={backPath}>
                  <ArrowLeftOutlined
                    style={{
                      color: primaryTheme.token?.colorPrimary,
                      fontSize: 15,
                    }}
                  />
                </Link>
              )}
              <DocuAtomText fontSize={16} weight={600} data-testid="heading">
                {pageTitle}
              </DocuAtomText>
            </DocuAtomFlex>
            {pageTitleRightContent}
          </DocuAtomFlex>
        </>
      )}
      <div className={styles.pageContent}>{children}</div>
    </Content>
  );
};

export default PageContainer;
