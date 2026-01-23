import { Avatar, AvatarProps } from "antd";
import React from "react";

export type DocuAtomAvatarProps = AvatarProps;

const DocuAtomAvatar = (props: DocuAtomAvatarProps) => {
  return <Avatar {...props} />;
};

export default DocuAtomAvatar;
