package gaia.graphicdiff.differs;

import lombok.Value;

@Value
public class FieldChange {
    private String key;
    private String oldValue;
    private String newValue;

    @Override
    public String toString() {
        return String.format("%-50s: { oldValue: %-30s, newValue: %-30s }", key, oldValue, newValue);
    }
}
