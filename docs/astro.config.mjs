import {defineConfig} from 'astro/config';
import starlight from '@astrojs/starlight';
import starlightThemeSix from '@six-tech/starlight-theme-six'

export default defineConfig({
    base: '/Six.SvgKit/', // URL fragment for GitHub pages (your repo name)
    site: 'https://six-tech.github.io', // Base URL for your site (your GitHub pages URL)
    integrations: [
        starlight({
            title: 'SVG Kit',
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
                    label: 'SVG Kit',
                    items: [
                        {label: 'Introduction', slug: 'svgkit/introduction'},
                        {label: 'Getting Started', slug: 'svgkit/getting-started'},
                        {
                            label: 'API Reference',
                            items: [
                                {label: 'Overview', slug: 'svgkit/api-reference'},
                                {label: 'Svg Class', slug: 'svgkit/api-reference/svg-class'},
                                {label: 'SvgQueryBuilder Class', slug: 'svgkit/api-reference/svg-query-builder'},
                                {label: 'SvgNode Class', slug: 'svgkit/api-reference/svg-node'},
                                {label: 'SvgResult Class', slug: 'svgkit/api-reference/svg-result'},
                                {label: 'Extension Methods', slug: 'svgkit/api-reference/extension-methods'},
                                {label: 'Format Conversion', slug: 'svgkit/api-reference/format-conversion'}
                            ],
                        },
                        {
                            label: 'Optimization Guide',
                            items: [
                                {label: 'Overview', slug: 'svgkit/optimization'},
                                {label: 'Path Optimization', slug: 'svgkit/optimization/path-optimization'},
                                {label: 'Cleanup Operations', slug: 'svgkit/optimization/cleanup-operations'},
                                {label: 'Output Control', slug: 'svgkit/optimization/output-control'},
                                {label: 'Advanced Strategies', slug: 'svgkit/optimization/advanced-strategies'},
                                {label: 'Best Practices', slug: 'svgkit/optimization/best-practices'},
                                {label: 'Performance', slug: 'svgkit/optimization/performance'},
                                {label: 'Troubleshooting', slug: 'svgkit/optimization/troubleshooting'}
                            ],
                        },
                        {
                            label: 'Examples',
                            items: [
                                {label: 'Overview', slug: 'svgkit/examples'},
                                {label: 'Basic Operations', slug: 'svgkit/examples/basic-operations'},
                                {label: 'Simple Optimizations', slug: 'svgkit/examples/simple-optimizations'},
                                {label: 'Icon Processing', slug: 'svgkit/examples/icon-processing'},
                                {label: 'Batch Processing', slug: 'svgkit/examples/batch-processing'},
                                {label: 'Advanced Scenarios', slug: 'svgkit/examples/advanced-scenarios'},
                                {label: 'Format Conversion', slug: 'svgkit/examples/format-conversion'},
                                {label: 'Integration Examples', slug: 'svgkit/examples/integration-examples'}
                            ],
                        }
                    ],
                },
                {
                    label: 'Six',
                    items: [
                        {
                            label: 'Introduction',
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
                },
            ],

        }),
    ],
});
