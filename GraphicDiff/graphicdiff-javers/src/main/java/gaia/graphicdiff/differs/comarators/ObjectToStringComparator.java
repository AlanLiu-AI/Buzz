package gaia.graphicdiff.differs.comarators;

import org.javers.core.diff.changetype.ValueChange;
import org.javers.core.diff.custom.CustomPropertyComparator;
import org.javers.core.metamodel.object.GlobalId;
import org.javers.core.metamodel.property.Property;

public class ObjectToStringComparator<T>
    implements CustomPropertyComparator<T, ValueChange> {

    @Override
    public ValueChange compare(T left, T right, GlobalId affectedId, Property property) {
        String leftToString = toString(left);
        String rightToString = toString(right);

        if (leftToString.equals(rightToString)){
            return null;
        }

        return new ValueChange(affectedId, property.getName(), leftToString, rightToString);
    }

    private String toString(Object object) {
        return object != null ? object.toString() : "";
    }
}
