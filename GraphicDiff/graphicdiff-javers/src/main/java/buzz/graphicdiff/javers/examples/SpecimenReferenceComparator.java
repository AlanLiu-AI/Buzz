package buzz.graphicdiff.javers.examples;

import org.javers.core.diff.changetype.ValueChange;
import org.javers.core.diff.custom.CustomPropertyComparator;
import org.javers.core.metamodel.object.GlobalId;
import org.javers.core.metamodel.property.Property;

import gaia.domain.model.Specimen;

public class SpecimenReferenceComparator
    implements CustomPropertyComparator<Specimen, ValueChange> {

    @Override
    public ValueChange compare(Specimen left, Specimen right, GlobalId affectedId, Property property) {
        String leftName = getName(left);
        String rightName = getName(right);

        if (leftName.equals(rightName)){
            return null;
        }

        return new ValueChange(affectedId, property.getName(), leftName, rightName);
    }

    private String getName(Specimen specimen) {
        return specimen != null ? specimen.getName() : "";
    }
}
