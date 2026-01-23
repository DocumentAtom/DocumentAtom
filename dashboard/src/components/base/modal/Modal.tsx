import { Modal, ModalProps } from "antd";
import React from "react";

export type DocuAtomModalProps = ModalProps & {};
const DocuAtomModal = (props: DocuAtomModalProps) => {
  return <Modal {...props} />;
};

export default DocuAtomModal;
