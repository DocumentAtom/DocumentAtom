import { Form, FormItemProps } from "antd";
import React from "react";
const DocuAtomFormItem = (props: FormItemProps) => {
  const { className, ...rest } = props;
  return <Form.Item className={className} {...rest}></Form.Item>;
};

export default DocuAtomFormItem;
