# .github/pull_request_template.md

## 📋 Description
<!-- Provide a clear and concise description of what this PR accomplishes -->

## 🔗 Related Issue
<!-- If this PR fixes an issue, link it here -->
Closes #(issue_number)

## 📝 Type of Change
<!-- Please delete options that are not relevant -->
- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📚 Documentation update
- [ ] ♻️ Code refactor (no functional changes)
- [ ] 🧪 Test update
- [ ] 🔧 Build/CI configuration change
- [ ] 🎨 Style/Formatting (code style, formatting, missing semicolons, etc.)

## ✅ Checklist
<!-- Please check all items that apply -->
- [ ] My code follows the project's coding style guidelines
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings or errors
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] All existing and new tests pass locally (`dotnet test`)
- [ ] Code coverage is ≥ 80%
- [ ] PR title follows [Conventional Commits](https://www.conventionalcommits.org/) format

## 🧪 Testing Instructions
<!-- Describe how to test your changes -->
1. 
2. 
3. 

## 📸 Screenshots (if applicable)
<!-- Add screenshots to help explain your changes -->

## 🔍 Additional Context
<!-- Add any other context about the PR here -->
- [ ] This PR depends on another PR: #(PR_number)
- [ ] This PR includes database migrations

## 🚀 Deployment Notes
<!-- Any special deployment considerations? -->
- [ ] Environment variables need to be updated
- [ ] Database migration required
- [ ] Service restart required

---

## 📊 Code Review Checklist for Reviewers

| Area | Check |
|------|-------|
| Architecture | ✅ Follows Clean Architecture (Domain → Application → Infrastructure) |
| Security | ✅ No hardcoded secrets. All sensitive config via environment variables |
| Performance | ✅ No N+1 queries. Proper use of async/await |
| Error Handling | ✅ Proper exception handling. No swallowed exceptions |
| Logging | ✅ Appropriate log levels. No sensitive data in logs |
| Testing | ✅ Unit tests cover business logic. Integration tests cover data access |
| Documentation | ✅ XML comments for public APIs. Updated README if needed |