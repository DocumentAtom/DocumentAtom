import React from "react";
import DocuAtomFlex from "../base/flex/Flex";
import Image from "next/image";
import DocuAtomText from "../base/typograpghy/Text";

const DocuAtomLogo = ({
  showOnlyIcon,
  size = 20,
  imageSize = 45,
}: {
  showOnlyIcon?: boolean;
  size?: number;
  imageSize?: number;
}) => {
  return (
    <DocuAtomFlex align="center" gap={10}>
      <Image
        src={"/assets/logo.png"}
        alt="Document Atom"
        height={imageSize}
        width={imageSize}
      />
      {!showOnlyIcon && (
        <DocuAtomText fontSize={size}>Document Atom</DocuAtomText>
      )}
    </DocuAtomFlex>
  );
};

export default DocuAtomLogo;
