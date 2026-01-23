"use client";
import { CopyOutlined } from "@ant-design/icons";
import React, { useState } from "react";
import DocuAtomFlex from "../base/flex/Flex";
import DocuAtomText from "../base/typograpghy/Text";
import DocuAtomTooltip from "../base/tooltip/Tooltip";
import classNames from "classnames";
import DocuAtomButton from "../base/button/Button";

interface TextWithCopyProps {
  text: string;
  className?: string;
}

const TextWithCopy = ({ text, className }: TextWithCopyProps) => {
  const [isCopied, setIsCopied] = useState(false);

  const handleCopy = () => {
    navigator.clipboard.writeText(text);
    setIsCopied(true);
    setTimeout(() => {
      setIsCopied(false);
    }, 2000);
  };

  return (
    <DocuAtomFlex
      //   style={{ display: "inline-flex" }}
      align="center"
      gap={10}
      className={classNames(className, "mb-0")}
    >
      <DocuAtomText>{text}</DocuAtomText>
      <DocuAtomTooltip
        title={isCopied ? "Copied" : "Copy"}
        placement="top"
        color={isCopied ? "success" : "default"}
      >
        <DocuAtomButton
          type="link"
          icon={<CopyOutlined />}
          onClick={handleCopy}
        />
      </DocuAtomTooltip>
    </DocuAtomFlex>
  );
};

export default TextWithCopy;
