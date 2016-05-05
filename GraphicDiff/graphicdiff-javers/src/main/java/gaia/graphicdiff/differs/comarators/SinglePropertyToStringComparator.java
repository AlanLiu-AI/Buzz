package gaia.graphicdiff.differs.comarators;

import org.apache.commons.lang3.reflect.FieldUtils;
import org.javers.core.diff.changetype.ValueChange;
import org.javers.core.diff.custom.CustomPropertyComparator;
import org.javers.core.metamodel.object.GlobalId;
import org.javers.core.metamodel.property.Property;

public class SinglePropertyToStringComparator<T>
    implements CustomPropertyComparator<T, ValueChange> {

    private final String fieldName;

    public SinglePropertyToStringComparator(String fieldName) {
        this.fieldName = fieldName;
    }

    @Override
    public ValueChange compare(T left, T right, GlobalId affectedId, Property property) {
        String leftName = getPropertyAsString(left);
        String rightName = getPropertyAsString(right);

        if (leftName.equals(rightName)){
            return null;
        }

        return new ValueChange(affectedId, property.getName(), leftName, rightName);
    }

    private String getPropertyAsString(T domainObject) {
        String value = "";
        if (domainObject != null) {
            try {
                Object fieldValue = FieldUtils.readField(domainObject, fieldName, true);
                if (fieldValue != null) {
                    return fieldValue.toString();
                }
            } catch (IllegalAccessException e) {
                throw new RuntimeException(e);
            }
        }
        return value;
    }
}
