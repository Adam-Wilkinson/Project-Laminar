namespace Laminar.Domain.Exceptions;

public class UndoRedoClashException() : Exception("Undo/Redo operation caught an unresolvable error. Some editors may be in an invalid state");