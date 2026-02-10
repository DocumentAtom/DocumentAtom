from typing import Any, Dict, Optional

from pydantic import BaseModel, ConfigDict, Field


class TypeDetectionResultModel(BaseModel):
    """
    Result from type detection API.
    """

    file_type: str = Field(alias="FileType")
    confidence: Optional[float] = Field(default=None, alias="Confidence")
    metadata: Optional[Dict[str, Any]] = Field(default=None, alias="Metadata")
    model_config = ConfigDict(populate_by_name=True)
