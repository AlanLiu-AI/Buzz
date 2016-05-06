package gaia.graphicdiff.differs.comarators;

import java.lang.reflect.Field;

import org.apache.commons.lang3.reflect.FieldUtils;
import org.javers.core.diff.changetype.ValueChange;
import org.javers.core.diff.custom.CustomPropertyComparator;
import org.javers.core.metamodel.object.GlobalId;
import org.javers.core.metamodel.property.Property;

public class SinglePropertyToStringComparator<T>
    implements CustomPropertyComparator<T, ValueChange> {

    private final Field field;

    protected SinglePropertyToStringComparator(Class<T> type, String fieldName) {
        this.field = FieldUtils.getField(type, fieldName, true);
    }

    public static <T> SinglePropertyToStringComparator<T> createComparator(Class<T> type, String fieldName) {
        return new SinglePropertyToStringComparator<>(type, fieldName);
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
                Object fieldValue = FieldUtils.readField(field, domainObject);
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
