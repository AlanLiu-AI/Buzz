EPA SEDD(Staged Electronic Data Deliverable) Documents:

http://www.epa.gov/osainter/fem/seddspec52.htm

SEDD DTD schema:
http://www.epa.gov/osainter/fem/pdfs/sedd52_dtds_2.zip


The SEDDD schema is DTD, to get better gernerated source code, here we convert it to XSD and then use the XSD for JAXB code generation.
To do that, please go with following script:
$ curl http://www.w3.org/2000/04/schema_hack/dtd2xsd.pl -o dtd2xsd.pl
$ perl dtd2xsd.pl -prefix sedd -ns http://www.epa.gov/osainter/fem/seddspec5 -pcdata -simpletype ContentType string  SEDD_5-2_GENERAL_3_2.dtd >SEDD_5-2_GENERAL_3_2.xsd

