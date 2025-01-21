import os
import argparse
import sys
import time
from elevenlabs.client import ElevenLabs
from dotenv import load_dotenv

load_dotenv()
#sk_f126364b713b29b77320fafce48940e5e043de7a7cfc75ba
#elevenlabs = ElevenLabs(api_key=os.getenv("ELEVENLABS_API_KEY"))
elevenlabs = ElevenLabs(api_key="sk_f126364b713b29b77320fafce48940e5e043de7a7cfc75ba")
current_dir = os.path.dirname(os.path.abspath(sys.argv[0]))

output_path = os.path.join(current_dir, "..\\", "..\\",  "source\\")

def generate_sound_effect(text: str, output_file_name: str, sample_time: int):
    print("Generating sound effects...")

    result = elevenlabs.text_to_sound_effects.convert(
        text=text,
        duration_seconds=sample_time,  # Optional, if not provided will automatically determine the correct length
        prompt_influence=0.3,  # Optional, if not provided will use the default value of 0.3
    )
    print("Python Enter")
    with open(output_path + output_file_name, "wb") as f:
        for chunk in result:
            f.write(chunk)

    print(f"Audio saved to {output_file_name}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Generate sound effects using ElevenLabs API.")
    parser.add_argument("--text", type=str, help="The text to generate sound effects from.")
    parser.add_argument("--output_file_name", type=str, help="The name of the output file.")
    parser.add_argument("--sample_time", type=int, help="The sample time in seconds.")

    args = parser.parse_args()

    sample_time = int(args.sample_time)
    t1 = str(args.text)
    t2 = str(args.output_file_name)
    # print(f"Text: {args.text}")
    # print(f"Output File Name: {args.output_file_name}")
    # print(f"Sample Time: {args.sample_time} seconds")
    print("python OK")
    generate_sound_effect(t1, t2+".mp3", args.sample_time)#+".wav"
    # print(time.ctime)
    # time.sleep(10)
    # print(time.ctime)