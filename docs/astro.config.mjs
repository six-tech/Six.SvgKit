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
                            link: '/svgkit/introduction',
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
                    label: 'Introduction',
                    items: [
                        {label: 'Welcome', slug: 'introduction'},
                        {label: 'Getting Started', slug: 'getting-started'},
                    ]
                },
                {
                    label: 'API Reference',
                    items: [
                        {label: 'Overview', slug: 'api-reference'},
                        {label: 'Svg Class', slug: 'api-reference/svg-class'},
                        {label: 'SvgQueryBuilder Class', slug: 'api-reference/svg-query-builder'},
                        {label: 'SvgNode Class', slug: 'api-reference/svg-node'},
                        {label: 'SvgResult Class', slug: 'api-reference/svg-result'},
                        {label: 'Extension Methods', slug: 'api-reference/extension-methods'},
                        {label: 'Format Conversion', slug: 'api-reference/format-conversion'}
                    ],
                },
                {
                    label: 'Optimization Guide',
                    items: [
                        {label: 'Overview', slug: 'optimization'},
                        {label: 'Path Optimization', slug: 'optimization/path-optimization'},
                        {label: 'Cleanup Operations', slug: 'optimization/cleanup-operations'},
                        {label: 'Output Control', slug: 'optimization/output-control'},
                        {label: 'Advanced Strategies', slug: 'optimization/advanced-strategies'},
                        {label: 'Best Practices', slug: 'optimization/best-practices'},
                        {label: 'Performance', slug: 'optimization/performance'},
                        {label: 'Troubleshooting', slug: 'optimization/troubleshooting'}
                    ],
                },
                {
                    label: 'Examples',
                    items: [
                        {label: 'Overview', slug: 'examples'},
                        {label: 'Basic Operations', slug: 'examples/basic-operations'},
                        {label: 'Simple Optimizations', slug: 'examples/simple-optimizations'},
                        {label: 'Icon Processing', slug: 'examples/icon-processing'},
                        {label: 'Batch Processing', slug: 'examples/batch-processing'},
                        {label: 'Advanced Scenarios', slug: 'examples/advanced-scenarios'},
                        {label: 'Format Conversion', slug: 'examples/format-conversion'},
                        {label: 'Integration Examples', slug: 'examples/integration-examples'}
                    ],
                }
            ],

        }),
    ],
});
