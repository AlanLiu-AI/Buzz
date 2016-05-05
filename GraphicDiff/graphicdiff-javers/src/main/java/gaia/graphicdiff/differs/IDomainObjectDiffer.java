package gaia.graphicdiff.differs;

import java.util.List;

public interface IDomainObjectDiffer<T> {

    List<FieldChange> getFieldChanges(T left, T right);
}
