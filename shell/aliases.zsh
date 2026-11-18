# Claude Code aliases that load the toolkit's system prompts.
# Source this file from ~/.zshrc. Set PERSONAL_AI_TOOLKIT first if the toolkit lives somewhere else.

export PERSONAL_AI_TOOLKIT="${PERSONAL_AI_TOOLKIT:-$HOME/code/personal-ai-toolkit}"

# c: everyday sessions, with the communication prompt.
alias c='claude --append-system-prompt-file "$PERSONAL_AI_TOOLKIT/prompts/system_prompt.md" --remote-control'

# cc: coding sessions, with the communication and coding prompts.
alias cc='claude --append-system-prompt "$(cat "$PERSONAL_AI_TOOLKIT/prompts/system_prompt.md" "$PERSONAL_AI_TOOLKIT/prompts/system_prompt_for_coding.md")" --remote-control'
