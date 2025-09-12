import {defineConfig} from 'astro/config';
import starlight from '@astrojs/starlight';
import starlightThemeSix from '@six-tech/starlight-theme-six'

export default defineConfig({
    base: '/Six.SvgKit/', // URL fragment for GitHub pages (your repo name)
    site: 'https://six-tech.github.io', // Base URL for your site (your GitHub pages URL)
    integrations: [
        starlight({
            title: 'Six SVGKit',
            description: '.NET library for optimizing SVG files for performance and accessibility.',
            editLink: {
                // Base url for edit links on GitHub. Basically edit-url to the docs' folder.
                baseUrl: 'https://github.com/six-tech/Six.SvgKit/edit/main/docs/',
            },
            social: [
                {
                    icon: 'github',
                    label: 'GitHub',
                    href: 'https://github.com/six-tech/Six.SvgKit'
                },
            ],
            plugins: [
                starlightThemeSix({
                    navLinks: [
                        {
                            label: 'Docs',
                            title: 'Link to main documentation page',
                            link: '/six/introduction',
                        },
                        {
                            label: 'Six',
                            title: 'Link to Six homepage',
                            link: 'https://six.technology',
                            attrs: {
                                target: '_blank',
                                rel: 'noopener',
                            },
                        },
                    ],
                }),
            ],
            sidebar: [
                {
                    label: 'Getting Started',
                    items: [
                        {label: 'Introduction', slug: 'six/introduction'},
                        {label: 'Setup and Configuration', slug: 'six/setup-and-configuration'}                        
                    ],
                },
                {
                    label: 'Structure',
                    items: [                        
                        {label: 'Overview', slug: 'six/structure'},
                        {label: 'Directories', slug: 'six/structure/str-directories'},
                        {label: 'Configuration', slug: 'six/structure/str-configuration'}
                    ],
                },
                {
                    label: 'Scripts',                    
                    items: [
                        {label: 'Overview', slug: 'six/scripts'},
                        {label: 'Setup Script', slug: 'six/scripts/setup'},
                        {label: 'Test Runner', slug: 'six/scripts/tests-run'},
                        {label: 'Coverage Collection', slug: 'six/scripts/tests-coverage'},
                        {label: 'Coverage Reports', slug: 'six/scripts/tests-report'},
                        {label: 'NuGet Packaging', slug: 'six/scripts/nuget-pack'},
                    ],
                },
                {
                    label: 'Nuget Feeds',
                    items: [
                        {label: 'Nuget Secrets', slug: 'six/nuget-feeds/nuget-secrets'},
                        {label: 'Nuget Feeds', slug: 'six/nuget-feeds/nuget-feeds'}
                    ],
                },
                {
                    label: 'Documentation',
                    items: [
                        {label: 'Setting Up', slug: 'six/project-documentation/setting-up-documentation'},
                        {label: 'Creating Documents', slug: 'six/project-documentation/creating-documentation'},
                        {label: 'Documentation with AI', slug: 'six/project-documentation/using-with-ai'},
                    ],
                },
                {
                    label: 'Github Workflows',
                    items: [
                        {label: 'Overview', slug: 'six/github-workflow-integration'},
                        {label: 'Test Workflow', slug: 'six/github-workflow-integration/github-test-workflow'},
                        {
                            label: 'Documentation Deploy',
                            slug: 'six/github-workflow-integration/github-documentation-workflow'
                        },
                    ],
                },
                {
                    label: 'AI',
                    items: [
                        {label: 'Overview', slug: 'six/ai/introduction'},
                        {label: 'AGENTS.md', slug: 'six/ai/agents-markdown'},
                        {
                            label: 'Cursor',
                            items: [
                                {label: 'Overview', slug: 'six/ai/cursor-integration/cursor-introduction'},
                                {label: '.NET Rules', slug: 'six/ai/cursor-integration/dotnet-rules'},
                                {label: 'Web Rules', slug: 'six/ai/cursor-integration/web-rules'},
                                {label: 'Documentation Rules', slug: 'six/ai/cursor-integration/documentation-rules'},
                                {label: 'Specialized Rules', slug: 'six/ai/cursor-integration/specialized-rules'}
                            ],
                        },
                        {
                            label: 'Kiro',
                            items: [
                                {label: 'Overview', slug: 'six/ai/kiro-integration/kiro-introduction'},
                                {label: '.NET Steering Files', slug: 'six/ai/kiro-integration/dotnet-steering'},
                                {label: 'Web Steering Files', slug: 'six/ai/kiro-integration/web-steering'},
                                {label: 'Documentation Steering Files', slug: 'six/ai/kiro-integration/documentation-steering'},
                                {label: 'Kiro Hooks', slug: 'six/ai/kiro-integration/kiro-hooks'}
                            ],
                        },
                    ],
                },
            ],

        }),
    ],
});
