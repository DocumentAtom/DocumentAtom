import React from "react";
import { Upload, UploadProps } from "antd";
import styles from "./upload.module.scss";
import classNames from "classnames";

type DocuAtomUploadProps = UploadProps;

const DocuAtomUpload = (props: DocuAtomUploadProps) => {
  const { className, ...rest } = props;
  return (
    <Upload
      {...rest}
      className={classNames(styles.uploadContainer, className)}
    />
  );
};

export default DocuAtomUpload;
