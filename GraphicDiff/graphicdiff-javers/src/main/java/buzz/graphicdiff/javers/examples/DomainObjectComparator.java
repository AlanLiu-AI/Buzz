package buzz.graphicdiff.javers.examples;

import org.javers.core.diff.changetype.ValueChange;
import org.javers.core.diff.custom.CustomPropertyComparator;
import org.javers.core.metamodel.object.GlobalId;
import org.javers.core.metamodel.property.Property;

import gaia.domain.interfaces.IDomainObject;

public class DomainObjectComparator<T extends IDomainObject>
    implements CustomPropertyComparator<T, ValueChange> {

    @Override
    public ValueChange compare(T left, T right, GlobalId affectedId, Property property) {
        String leftCustomId = getCustomId(left);
        String rightCustomId = getCustomId(right);

        if (leftCustomId.equals(rightCustomId)){
            return null;
        }

        return new ValueChange(affectedId, property.getName(), leftCustomId, rightCustomId);
    }

    private String getCustomId(IDomainObject domainObject) {
        return domainObject != null ? domainObject.getCustomId() : "";
    }
}
