import { Card, CardProps } from "antd";
import React from "react";

export type DocuAtomCardProps = CardProps & {};
const DocuAtomCard = (props: DocuAtomCardProps) => {
  return <Card {...props} />;
};

export default DocuAtomCard;
