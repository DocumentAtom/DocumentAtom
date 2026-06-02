import React from "react";
import DocuAtomFlex from "../base/flex/Flex";
import Image from "next/image";
import DocuAtomText from "../base/typograpghy/Text";

const DocuAtomLogo = ({
  showOnlyIcon,
  size = 20,
  imageSize = 45,
  text = "Document Atom",
}: {
  showOnlyIcon?: boolean;
  size?: number;
  imageSize?: number;
  text?: string;
}) => {
  return (
    <DocuAtomFlex align="center" gap={10}>
      <Image
        src={"/assets/logo.png"}
        alt="Document Atom"
        height={imageSize}
        width={imageSize}
      />
      {!showOnlyIcon && <DocuAtomText fontSize={size}>{text}</DocuAtomText>}
    </DocuAtomFlex>
  );
};

export default DocuAtomLogo;
