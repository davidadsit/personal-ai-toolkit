# Claude Code aliases that load the toolkit's system prompts.
# Source this file from ~/.zshrc. Set these first to change the defaults:
#   PERSONAL_AI_TOOLKIT  where this toolkit is cloned
#   PERSONAL_AI_PROMPT   an optional file of your own additions, appended after the toolkit prompts

export PERSONAL_AI_TOOLKIT="${PERSONAL_AI_TOOLKIT:-$HOME/code/personal-ai-toolkit}"
export PERSONAL_AI_PROMPT="${PERSONAL_AI_PROMPT:-$HOME/.personal-ai-prompt.md}"

personal_ai_prompt() {
  cat "$@"
  if [ -f "$PERSONAL_AI_PROMPT" ]; then
    printf '\n'
    cat "$PERSONAL_AI_PROMPT"
  fi
}

# c: everyday sessions, with the communication prompt.
alias c='claude --append-system-prompt "$(personal_ai_prompt "$PERSONAL_AI_TOOLKIT/prompts/system_prompt.md")" --remote-control'

# cc: coding sessions, with the communication and coding prompts.
alias cc='claude --append-system-prompt "$(personal_ai_prompt "$PERSONAL_AI_TOOLKIT/prompts/system_prompt.md" "$PERSONAL_AI_TOOLKIT/prompts/system_prompt_for_coding.md")" --remote-control'
