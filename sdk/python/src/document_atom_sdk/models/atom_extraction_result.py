from typing import List

from pydantic import TypeAdapter

from .atom import AtomModel

# The server returns a JSON array of Atom objects (List<Atom>), not a wrapper object.
# Use a TypeAdapter to validate the list directly.
AtomListAdapter = TypeAdapter(List[AtomModel])
