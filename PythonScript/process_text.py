import sys
import json
import spacy

# Load the spaCy language model
nlp = spacy.load("en_core_web_sm")

# Read text input from the command-line arguments
text = sys.argv[1]

# Process text with spaCy
doc = nlp(text)
entities = [{"text": ent.text, "label": ent.label_} for ent in doc.ents]

# Print entities as a JSON string for the C# code to read
print(json.dumps({"entities": entities}))