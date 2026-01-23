import React from "react";
import DocuAtomModal from "../base/modal/Modal";
import DocuAtomButton from "../base/button/Button";
import DocuAtomParagraph from "../base/typograpghy/Paragraph";
import DocuAtomFlex from "../base/flex/Flex";

interface ConfirmationModalProps {
  title: string;
  isModelVisible: boolean;
  setIsModelVisible: (value: boolean) => void;
  handleConfirm: () => void;
  paragraphText: string;
  isLoading?: boolean;
}

const ConfirmationModal = ({
  isLoading,
  title,
  isModelVisible,
  setIsModelVisible,
  handleConfirm,
  paragraphText,
}: ConfirmationModalProps) => {
  return (
    <DocuAtomModal
      title={title}
      centered
      open={isModelVisible}
      onCancel={() => setIsModelVisible(false)}
      footer={
        <DocuAtomFlex justify="end" gap={10}>
          <DocuAtomButton
            data-testid="confirmation-modal-cancel-button"
            type="default"
            onClick={() => setIsModelVisible(false)}
          >
            Cancel
          </DocuAtomButton>
          <DocuAtomButton
            data-testid="confirmation-modal-confirm-button"
            type="primary"
            onClick={handleConfirm}
            loading={isLoading}
          >
            Confirm
          </DocuAtomButton>
        </DocuAtomFlex>
      }
    >
      <DocuAtomParagraph>{paragraphText}</DocuAtomParagraph>
    </DocuAtomModal>
  );
};

export default ConfirmationModal;
