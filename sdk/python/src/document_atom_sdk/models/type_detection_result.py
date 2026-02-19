from typing import Optional

from pydantic import BaseModel, ConfigDict, Field


class TypeDetectionResultModel(BaseModel):
    """
    Result from type detection API.
    """

    mime_type: Optional[str] = Field(default=None, alias="MimeType")
    extension: Optional[str] = Field(default=None, alias="Extension")
    type: Optional[str] = Field(default=None, alias="Type")
    model_config = ConfigDict(populate_by_name=True)
