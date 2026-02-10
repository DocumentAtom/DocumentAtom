from ..mixins import TypeDetectableAPIResource
from ..models.type_detection_result import TypeDetectionResultModel


class TypeDetection(TypeDetectableAPIResource):
    """
    Type detection resource class.
    """

    RESOURCE_NAME: str = "typedetect"
    MODEL = TypeDetectionResultModel

    @classmethod
    def detect_type(cls, file_input, filename=None):
        """
        Detect the type of a file.

        Args:
            file_input: File path (str), bytes, or file-like object (BinaryIO/BytesIO)
            filename: Optional filename (required if file_input is bytes or file-like object without a name)

        Returns:
            TypeDetectionResultModel containing detected file type and metadata
        """
        result = super().detect_type(file_input, filename)
        if cls.MODEL:
            return cls.MODEL.model_validate(result)
        return result
