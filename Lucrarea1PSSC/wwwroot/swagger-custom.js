// Custom Swagger UI Enhancements
// E-Commerce Cart API - Complete Professional Transformation

(function() {
    'use strict';

    // Wait for Swagger UI to load
    window.addEventListener('load', function() {
        console.log('?? E-Commerce Cart API loaded successfully!');
        
        // Initial transformations
        setTimeout(function() {
            transformAllText();
            improveTextReadability();
            addWelcomeBanner();
            addQuickStartGuide();
            enhanceEndpointDescriptions();
            addStatusIndicators();
            improveMethodLabels();
            enhanceResponseCodes();
            beautifySchemas();
            improveModels();
            enhanceHeaders();
        }, 500);

        // Continuous improvements every 3 seconds
        setInterval(function() {
            transformAllText();
            improveTextReadability();
            beautifySchemas();
            improveModels();
        }, 3000);
    });

    function transformAllText() {
        // Comprehensive text replacements
        const transformations = {
            // Technical to Human-Readable
            'string': 'Text',
            'String': 'Text',
            'integer': 'Number',
            'Integer': 'Number',
            'int32': 'Whole Number',
            'int64': 'Large Number',
            'boolean': 'Yes/No',
            'Boolean': 'Yes/No',
            'array': 'List',
            'Array': 'List',
            'object': 'Data',
            'Object': 'Data',
            'enum': 'Options',
            'Enum': 'Options',
            'decimal': 'Price/Amount',
            'Decimal': 'Price/Amount',
            'double': 'Decimal Number',
            'Double': 'Decimal Number',
            'float': 'Decimal',
            'Float': 'Decimal',
            'date-time': 'Date & Time',
            'date': 'Date',
            'uuid': 'Unique ID',
            'guid': 'Unique ID',
            'uri': 'Web Link',
            'url': 'Web Address',
            
            // Schema improvements
            'schema': 'Data Structure',
            'Schema': 'Data Structure',
            'schemas': 'Data Structures',
            'Schemas': 'Data Structures',
            'model': 'Data Format',
            'Model': 'Data Format',
            'models': 'Data Formats',
            'Models': 'Data Formats',
            
            // Status improvements
            'deprecated': 'No Longer Supported',
            'Deprecated': 'No Longer Supported',
            'required': 'Required',
            'Required': 'Required',
            'optional': 'Optional',
            'Optional': 'Optional',
            'nullable': 'Can Be Empty',
            'Nullable': 'Can Be Empty'
        };

        // Apply transformations to text nodes only (NOT input fields)
        function replaceInNode(node) {
            if (node.nodeType === Node.TEXT_NODE) {
                let text = node.textContent;
                let changed = false;
                
                Object.keys(transformations).forEach(function(key) {
                    const regex = new RegExp('\\b' + key + '\\b', 'g');
                    if (regex.test(text)) {
                        text = text.replace(regex, transformations[key]);
                        changed = true;
                    }
                });
                
                if (changed) {
                    node.textContent = text;
                }
            } else if (node.nodeType === Node.ELEMENT_NODE) {
                // IMPORTANT: Skip input fields, textareas, and editable elements
                const skipTags = ['INPUT', 'TEXTAREA', 'CODE', 'PRE', 'SCRIPT', 'STYLE'];
                const isEditable = node.isContentEditable || node.hasAttribute('contenteditable');
                
                if (!skipTags.includes(node.tagName) && !isEditable) {
                    Array.from(node.childNodes).forEach(replaceInNode);
                }
            }
        }

        // Apply to specific non-input areas only
        const safeSelectors = [
            '.info',
            '.opblock-summary-description',
            '.parameter__type',
            '.parameter__in',
            '.response-col_description',
            '.model-title',
            '.prop-type',
            '.opblock-tag',
            'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
            'p', 'span:not(.parameter__name)', 'label:not([for])',
            '.markdown'
        ];

        safeSelectors.forEach(function(selector) {
            document.querySelectorAll(selector).forEach(function(element) {
                replaceInNode(element);
            });
        });
    }

    function improveTextReadability() {
        // Make parameter type descriptions more readable
        setTimeout(function() {
            document.querySelectorAll('.parameter__type').forEach(function(el) {
                // Skip if it's inside an input or has been modified
                if (el.closest('input, textarea') || el.hasAttribute('data-enhanced')) {
                    return;
                }
                
                el.setAttribute('data-enhanced', 'true');
                const text = el.textContent.trim();
                
                // Format type descriptions
                if (text.includes('(') && text.includes(')')) {
                    const parts = text.split('(');
                    if (parts.length === 2) {
                        const type = parts[0].trim();
                        const format = parts[1].replace(')', '').trim();
                        el.innerHTML = `<span style="color: #22c55e; font-weight: 600;">${type}</span> <span style="color: #64748b; font-style: italic;">(${format})</span>`;
                    }
                }
            });

            // Add required badges
            document.querySelectorAll('.parameter__name.required').forEach(function(el) {
                if (!el.querySelector('.required-badge') && !el.closest('input, textarea')) {
                    const badge = document.createElement('span');
                    badge.className = 'required-badge';
                    badge.style.cssText = `
                        background: #ef4444;
                        color: white;
                        padding: 0.125rem 0.5rem;
                        border-radius: 4px;
                        font-size: 0.625rem;
                        margin-left: 0.5rem;
                        font-weight: 700;
                        text-transform: uppercase;
                        letter-spacing: 0.05em;
                    `;
                    badge.textContent = 'Required';
                    el.appendChild(badge);
                }
            });

            // Improve "in" labels (query, path, header, etc.)
            document.querySelectorAll('.parameter__in').forEach(function(el) {
                if (el.hasAttribute('data-enhanced') || el.closest('input, textarea')) {
                    return;
                }
                
                el.setAttribute('data-enhanced', 'true');
                const text = el.textContent.trim().toLowerCase();
                const icons = {
                    'query': '??',
                    'path': '??',
                    'header': '??',
                    'body': '??',
                    'form': '??',
                    'cookie': '??'
                };
                
                if (icons[text]) {
                    el.textContent = icons[text] + ' ' + el.textContent;
                }
            });
        }, 1000);
    }

    function beautifySchemas() {
        // Improve schema/model section headings
        document.querySelectorAll('.model-title').forEach(function(el) {
            if (!el.querySelector('.schema-icon') && !el.closest('input, textarea')) {
                const icon = document.createElement('span');
                icon.className = 'schema-icon';
                icon.style.cssText = `
                    font-size: 1.5rem;
                    margin-right: 0.5rem;
                `;
                icon.textContent = '??';
                el.insertBefore(icon, el.firstChild);
            }
        });

        // Make schema property names more readable
        document.querySelectorAll('.property-row').forEach(function(row) {
            const nameEl = row.querySelector('.property-name');
            if (nameEl && !nameEl.querySelector('.property-icon') && !nameEl.closest('input, textarea')) {
                const icon = document.createElement('span');
                icon.className = 'property-icon';
                icon.style.cssText = `
                    color: #2563eb;
                    margin-right: 0.25rem;
                    font-size: 0.875rem;
                `;
                icon.textContent = '?';
                nameEl.insertBefore(icon, nameEl.firstChild);
            }
        });

        // Improve model toggle text
        document.querySelectorAll('.model-toggle').forEach(function(el) {
            if (el.hasAttribute('data-enhanced') || el.closest('input, textarea')) {
                return;
            }
            
            el.setAttribute('data-enhanced', 'true');
            const text = el.textContent.trim();
            if (text.includes('Example') || text.includes('example')) {
                el.textContent = '??? View Example Data';
            } else if (text.includes('Model') || text.includes('model')) {
                el.textContent = '?? View Data Structure';
            }
        });

        // Format property types beautifully
        document.querySelectorAll('.prop-type').forEach(function(el) {
            if (el.hasAttribute('data-enhanced') || el.closest('input, textarea')) {
                return;
            }
            
            el.setAttribute('data-enhanced', 'true');
            const type = el.textContent.trim();
            
            // Add icons based on type
            const typeIcons = {
                'Text': '??',
                'string': '??',
                'Number': '??',
                'integer': '??',
                'Decimal': '??',
                'decimal': '??',
                'Yes/No': '??',
                'boolean': '??',
                'Date': '??',
                'date': '??',
                'List': '??',
                'array': '??',
                'Data': '??',
                'object': '??'
            };
            
            Object.keys(typeIcons).forEach(function(key) {
                if (type.toLowerCase().includes(key.toLowerCase())) {
                    if (!el.querySelector('.type-icon')) {
                        const icon = document.createElement('span');
                        icon.className = 'type-icon';
                        icon.style.marginRight = '0.25rem';
                        icon.textContent = typeIcons[key];
                        el.insertBefore(icon, el.firstChild);
                    }
                }
            });
        });
    }

    function improveModels() {
        // Make the Models section header beautiful
        const modelsHeading = Array.from(document.querySelectorAll('h4, h3, h2')).find(el => 
            el.textContent.trim().toLowerCase().includes('model') || 
            el.textContent.trim().toLowerCase().includes('schema')
        );
        
        if (modelsHeading && !modelsHeading.querySelector('.models-icon') && !modelsHeading.closest('input, textarea')) {
            modelsHeading.innerHTML = `
                <span class="models-icon" style="font-size: 1.75rem; margin-right: 0.75rem;">??</span>
                <span style="background: linear-gradient(135deg, #2563eb, #7c3aed); 
                             -webkit-background-clip: text; 
                             -webkit-text-fill-color: transparent;
                             font-weight: 800;">
                    Data Structures & Examples
                </span>
            `;
        }

        // Improve model container styling
        document.querySelectorAll('.model-container').forEach(function(container) {
            if (!container.classList.contains('enhanced') && !container.closest('input, textarea')) {
                container.classList.add('enhanced');
                container.style.cssText = `
                    background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
                    border: 2px solid #e2e8f0;
                    border-radius: 12px;
                    padding: 1.5rem;
                    margin: 1rem 0;
                    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
                `;
            }
        });
    }

    function enhanceHeaders() {
        // Make section headers more beautiful
        document.querySelectorAll('h4').forEach(function(h4) {
            if (h4.closest('input, textarea') || h4.hasAttribute('data-enhanced')) {
                return;
            }
            
            h4.setAttribute('data-enhanced', 'true');
            const text = h4.textContent.trim().toLowerCase();
            
            const headerStyles = {
                'parameter': { icon: '??', color: '#2563eb', text: 'Request Parameters' },
                'request': { icon: '??', color: '#10b981', text: 'Request Details' },
                'response': { icon: '??', color: '#f59e0b', text: 'Response Information' },
                'header': { icon: '??', color: '#8b5cf6', text: 'Headers' },
                'example': { icon: '??', color: '#ec4899', text: 'Example Data' }
            };
            
            Object.keys(headerStyles).forEach(function(key) {
                if (text.includes(key) && !h4.querySelector('.header-icon')) {
                    const style = headerStyles[key];
                    h4.innerHTML = `
                        <span class="header-icon" style="font-size: 1.25rem; margin-right: 0.5rem;">${style.icon}</span>
                        <span style="color: ${style.color}; font-weight: 700;">${style.text}</span>
                    `;
                }
            });
        });
    }

    function addWelcomeBanner() {
        const infoContainer = document.querySelector('.information-container');
        if (infoContainer && !document.querySelector('.custom-banner')) {
            const banner = document.createElement('div');
            banner.className = 'custom-banner';
            banner.innerHTML = `
                <div style="background: linear-gradient(135deg, #2563eb 0%, #7c3aed 100%); 
                            color: white; 
                            padding: 2.5rem; 
                            border-radius: 16px; 
                            margin-bottom: 2rem;
                            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);">
                    <h2 style="margin: 0 0 1rem 0; 
                               font-size: 2rem; 
                               font-weight: 800;
                               letter-spacing: -0.02em;">
                        ?? Welcome to Your Professional E-Commerce API
                    </h2>
                    <p style="margin: 0 0 1.5rem 0; 
                              font-size: 1.125rem; 
                              opacity: 0.95; 
                              line-height: 1.7;
                              max-width: 800px;">
                        Your complete shopping cart management solution with real-time inventory tracking, 
                        secure transactions, and comprehensive database integration. Built with modern 
                        architecture patterns for maximum reliability and performance.
                    </p>
                    <div style="display: flex; gap: 1rem; flex-wrap: wrap; margin-bottom: 1rem;">
                        <span style="background: rgba(255,255,255,0.25); 
                                     padding: 0.625rem 1.25rem; 
                                     border-radius: 8px;
                                     font-size: 0.9375rem;
                                     font-weight: 600;
                                     backdrop-filter: blur(10px);">
                            ? Real-time Updates
                        </span>
                        <span style="background: rgba(255,255,255,0.25); 
                                     padding: 0.625rem 1.25rem; 
                                     border-radius: 8px;
                                     font-size: 0.9375rem;
                                     font-weight: 600;
                                     backdrop-filter: blur(10px);">
                            ?? Secure & Reliable
                        </span>
                        <span style="background: rgba(255,255,255,0.25); 
                                     padding: 0.625rem 1.25rem; 
                                     border-radius: 8px;
                                     font-size: 0.9375rem;
                                     font-weight: 600;
                                     backdrop-filter: blur(10px);">
                            ?? Complete Analytics
                        </span>
                        <span style="background: rgba(255,255,255,0.25); 
                                     padding: 0.625rem 1.25rem; 
                                     border-radius: 8px;
                                     font-size: 0.9375rem;
                                     font-weight: 600;
                                     backdrop-filter: blur(10px);">
                            ?? Database Integrated
                        </span>
                        <span style="background: rgba(255,255,255,0.25); 
                                     padding: 0.625rem 1.25rem; 
                                     border-radius: 8px;
                                     font-size: 0.9375rem;
                                     font-weight: 600;
                                     backdrop-filter: blur(10px);">
                            ?? Beautiful UI
                        </span>
                    </div>
                    <div style="margin-top: 1rem; 
                               padding-top: 1rem; 
                               border-top: 1px solid rgba(255,255,255,0.2);">
                        <span style="font-size: 0.875rem; opacity: 0.9;">
                            ?? <strong>Pro Tip:</strong> Start with the Quick Start Guide below to test the API in minutes!
                        </span>
                    </div>
                </div>
            `;
            infoContainer.insertBefore(banner, infoContainer.firstChild);
        }
    }

    function addQuickStartGuide() {
        const infoContainer = document.querySelector('.information-container');
        if (infoContainer && !document.querySelector('.quick-start-guide')) {
            const guide = document.createElement('div');
            guide.className = 'quick-start-guide';
            guide.innerHTML = `
                <div style="background: white; 
                            border: 2px solid #e2e8f0;
                            padding: 2rem; 
                            border-radius: 12px; 
                            margin-bottom: 2rem;
                            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);">
                    <h3 style="margin: 0 0 1.5rem 0; 
                               color: #0f172a; 
                               font-size: 1.5rem; 
                               font-weight: 800;
                               display: flex;
                               align-items: center;
                               gap: 0.5rem;">
                        <span style="font-size: 2rem;">??</span>
                        <span>Quick Start Guide - Get Running in 3 Steps</span>
                    </h3>
                    <div style="display: grid; 
                               grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); 
                               gap: 1.25rem;
                               margin-bottom: 1.5rem;">
                        <div style="padding: 1.5rem; 
                                   background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%); 
                                   border-radius: 10px;
                                   border: 2px solid #86efac;">
                            <div style="font-weight: 800; 
                                       color: #059669; 
                                       margin-bottom: 0.75rem; 
                                       font-size: 1.125rem;
                                       display: flex;
                                       align-items: center;
                                       gap: 0.5rem;">
                                <span style="background: #059669; 
                                           color: white; 
                                           width: 28px; 
                                           height: 28px; 
                                           border-radius: 50%; 
                                           display: inline-flex; 
                                           align-items: center; 
                                           justify-content: center;
                                           font-size: 0.875rem;">1</span>
                                <span>View Shopping Cart</span>
                            </div>
                            <div style="font-size: 0.9375rem; 
                                       color: #064e3b; 
                                       line-height: 1.6;
                                       margin-bottom: 0.75rem;">
                                See what's in a customer's cart with full details including items, prices, and status
                            </div>
                            <div style="background: rgba(255,255,255,0.7); 
                                       padding: 0.75rem; 
                                       border-radius: 6px;
                                       font-family: monospace;
                                       font-size: 0.8125rem;
                                       color: #064e3b;
                                       border-left: 3px solid #059669;">
                                GET /api/cart/view/Ion Popescu
                            </div>
                        </div>
                        <div style="padding: 1.5rem; 
                                   background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%); 
                                   border-radius: 10px;
                                   border: 2px solid #93c5fd;">
                            <div style="font-weight: 800; 
                                       color: #1d4ed8; 
                                       margin-bottom: 0.75rem; 
                                       font-size: 1.125rem;
                                       display: flex;
                                       align-items: center;
                                       gap: 0.5rem;">
                                <span style="background: #2563eb; 
                                           color: white; 
                                           width: 28px; 
                                           height: 28px; 
                                           border-radius: 50%; 
                                           display: inline-flex; 
                                           align-items: center; 
                                           justify-content: center;
                                           font-size: 0.875rem;">2</span>
                                <span>Add Products</span>
                            </div>
                            <div style="font-size: 0.9375rem; 
                                       color: #1e3a8a; 
                                       line-height: 1.6;
                                       margin-bottom: 0.75rem;">
                                Add items to cart with automatic stock verification and inventory management
                            </div>
                            <div style="background: rgba(255,255,255,0.7); 
                                       padding: 0.75rem; 
                                       border-radius: 6px;
                                       font-family: monospace;
                                       font-size: 0.8125rem;
                                       color: #1e3a8a;
                                       border-left: 3px solid #2563eb;">
                                POST /api/cart/add-product
                            </div>
                        </div>
                        <div style="padding: 1.5rem; 
                                   background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%); 
                                   border-radius: 10px;
                                   border: 2px solid #fbbf24;">
                            <div style="font-weight: 800; 
                                       color: #b45309; 
                                       margin-bottom: 0.75rem; 
                                       font-size: 1.125rem;
                                       display: flex;
                                       align-items: center;
                                       gap: 0.5rem;">
                                <span style="background: #f59e0b; 
                                           color: white; 
                                           width: 28px; 
                                           height: 28px; 
                                           border-radius: 50%; 
                                           display: inline-flex; 
                                           align-items: center; 
                                           justify-content: center;
                                           font-size: 0.875rem;">3</span>
                                <span>Process Payment</span>
                            </div>
                            <div style="font-size: 0.9375rem; 
                                       color: #78350f; 
                                       line-height: 1.6;
                                       margin-bottom: 0.75rem;">
                                Complete the checkout process and mark the cart as paid with transaction tracking
                            </div>
                            <div style="background: rgba(255,255,255,0.7); 
                                       padding: 0.75rem; 
                                       border-radius: 6px;
                                       font-family: monospace;
                                       font-size: 0.8125rem;
                                       color: #78350f;
                                       border-left: 3px solid #f59e0b;">
                                POST /api/cart/mark-paid
                            </div>
                        </div>
                    </div>
                    <div style="background: linear-gradient(135deg, #fef2f2 0%, #fee2e2 100%);
                               padding: 1.25rem; 
                               border-radius: 8px;
                               border-left: 4px solid #ef4444;">
                        <div style="display: flex; align-items: flex-start; gap: 0.75rem;">
                            <span style="font-size: 1.5rem;">??</span>
                            <div>
                                <strong style="color: #991b1b; 
                                             font-size: 1rem; 
                                             display: block; 
                                             margin-bottom: 0.25rem;">
                                    Ready-to-Use Test Data Available
                                </strong>
                                <span style="color: #7f1d1d; font-size: 0.9375rem; line-height: 1.6;">
                                    We've prepared 5 test customers (Ion Popescu, Maria Ionescu, Andrei Stanciu, Elena Radu, Mihai Popa) 
                                    and 10 products with real stock levels. Use them to test the API immediately!
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            infoContainer.appendChild(guide);
        }
    }

    function enhanceEndpointDescriptions() {
        const endpoints = {
            'view': { 
                icon: '???', 
                color: '#3b82f6', 
                label: 'VIEW', 
                description: 'Retrieve Cart Data',
                category: 'Read Operation'
            },
            'add-product': { 
                icon: '?', 
                color: '#22c55e', 
                label: 'CREATE', 
                description: 'Add New Item',
                category: 'Write Operation'
            },
            'mark-paid': { 
                icon: '??', 
                color: '#f59e0b', 
                label: 'UPDATE', 
                description: 'Process Payment',
                category: 'Transaction'
            },
            'active-carts': { 
                icon: '??', 
                color: '#8b5cf6', 
                label: 'ANALYTICS', 
                description: 'View All Carts',
                category: 'Administrator'
            },
            'health': { 
                icon: '??', 
                color: '#10b981', 
                label: 'STATUS', 
                description: 'System Health',
                category: 'Monitoring'
            }
        };

        Object.keys(endpoints).forEach(function(key) {
            const pathElements = document.querySelectorAll('.opblock-summary-path');
            pathElements.forEach(function(el) {
                if (el.textContent.includes(key) && !el.querySelector('.custom-icon')) {
                    const endpoint = endpoints[key];
                    
                    // Add icon
                    const icon = document.createElement('span');
                    icon.className = 'custom-icon';
                    icon.style.cssText = `
                        margin-right: 0.5rem; 
                        font-size: 1.5rem;
                        display: inline-flex;
                        align-items: center;
                    `;
                    icon.textContent = endpoint.icon;
                    el.insertBefore(icon, el.firstChild);

                    // Add label badge
                    const badge = document.createElement('span');
                    badge.style.cssText = `
                        background: ${endpoint.color};
                        color: white;
                        padding: 0.375rem 0.75rem;
                        border-radius: 6px;
                        font-size: 0.75rem;
                        font-weight: 700;
                        margin-left: 0.75rem;
                        letter-spacing: 0.05em;
                        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                    `;
                    badge.textContent = endpoint.label;
                    el.appendChild(badge);

                    // Add description
                    const desc = document.createElement('span');
                    desc.style.cssText = `
                        color: #475569;
                        font-size: 0.875rem;
                        margin-left: 0.75rem;
                        font-weight: 500;
                    `;
                    desc.textContent = endpoint.description;
                    el.appendChild(desc);

                    // Add category tag
                    const category = document.createElement('span');
                    category.style.cssText = `
                        background: #f1f5f9;
                        color: #64748b;
                        padding: 0.25rem 0.625rem;
                        border-radius: 4px;
                        font-size: 0.6875rem;
                        margin-left: 0.5rem;
                        font-weight: 600;
                        text-transform: uppercase;
                        letter-spacing: 0.05em;
                    `;
                    category.textContent = endpoint.category;
                    el.appendChild(category);
                }
            });
        });
    }

    function improveMethodLabels() {
        const methodLabels = {
            'GET': { name: 'Retrieve', description: 'Fetch existing data from the server' },
            'POST': { name: 'Create', description: 'Send new data to create a resource' },
            'PUT': { name: 'Update', description: 'Replace existing data completely' },
            'PATCH': { name: 'Modify', description: 'Update specific fields only' },
            'DELETE': { name: 'Remove', description: 'Delete existing resource' }
        };

        setTimeout(function() {
            document.querySelectorAll('.opblock-summary-method').forEach(function(el) {
                const method = el.textContent.trim();
                if (methodLabels[method]) {
                    el.setAttribute('title', `${methodLabels[method].name}: ${methodLabels[method].description}`);
                    el.style.cursor = 'help';
                }
            });
        }, 1000);
    }

    function enhanceResponseCodes() {
        const responseCodes = {
            '200': { 
                icon: '?', 
                title: 'Success', 
                description: 'Request completed successfully without any errors'
            },
            '201': { 
                icon: '?', 
                title: 'Created', 
                description: 'New resource was created successfully'
            },
            '204': { 
                icon: '?', 
                title: 'No Content', 
                description: 'Success but no data to return'
            },
            '400': { 
                icon: '?', 
                title: 'Bad Request', 
                description: 'Invalid request data - please check your input'
            },
            '401': { 
                icon: '??', 
                title: 'Unauthorized', 
                description: 'Authentication is required to access this resource'
            },
            '403': { 
                icon: '??', 
                title: 'Forbidden', 
                description: 'You don\'t have permission to access this'
            },
            '404': { 
                icon: '??', 
                title: 'Not Found', 
                description: 'The requested resource doesn\'t exist'
            },
            '500': { 
                icon: '??', 
                title: 'Server Error', 
                description: 'Something went wrong on our server'
            }
        };

        setTimeout(function() {
            document.querySelectorAll('.response-col_status').forEach(function(el) {
                if (el.closest('input, textarea') || el.hasAttribute('data-enhanced')) {
                    return;
                }
                
                el.setAttribute('data-enhanced', 'true');
                const code = el.textContent.trim();
                if (responseCodes[code]) {
                    const info = responseCodes[code];
                    el.innerHTML = `
                        <div style="display: flex; align-items: center; gap: 0.5rem;">
                            <span style="font-size: 1.25rem;">${info.icon}</span>
                            <div>
                                <div style="font-weight: 700; font-size: 1rem;">${code} - ${info.title}</div>
                                <div class="status-description" style="font-size: 0.75rem; font-weight: 400; margin-top: 0.25rem; line-height: 1.4; opacity: 0.9;">
                                    ${info.description}
                                </div>
                            </div>
                        </div>
                    `;
                }
            });
        }, 1500);
    }

    function addStatusIndicators() {
        const topbar = document.querySelector('.topbar');
        if (topbar && !document.querySelector('.status-indicator')) {
            const status = document.createElement('div');
            status.className = 'status-indicator';
            status.style.cssText = `
                position: absolute;
                right: 2rem;
                top: 50%;
                transform: translateY(-50%);
                display: flex;
                align-items: center;
                gap: 0.625rem;
                background: rgba(255, 255, 255, 0.15);
                padding: 0.625rem 1.25rem;
                border-radius: 8px;
                backdrop-filter: blur(10px);
                border: 1px solid rgba(255, 255, 255, 0.2);
            `;
            
            fetch('/api/cart/health')
                .then(response => response.json())
                .then(data => {
                    status.innerHTML = `
                        <span style="width: 10px; 
                                     height: 10px; 
                                     background: #22c55e; 
                                     border-radius: 50%;
                                     display: inline-block;
                                     animation: pulse 2s infinite;
                                     box-shadow: 0 0 10px rgba(34, 197, 94, 0.5);"></span>
                        <span style="color: white; 
                                     font-size: 0.9375rem; 
                                     font-weight: 700;
                                     letter-spacing: 0.025em;">
                            API Service Online
                        </span>
                    `;
                    
                    const style = document.createElement('style');
                    style.textContent = `
                        @keyframes pulse {
                            0%, 100% { opacity: 1; transform: scale(1); }
                            50% { opacity: 0.6; transform: scale(0.95); }
                        }
                    `;
                    document.head.appendChild(style);
                })
                .catch(() => {
                    status.innerHTML = `
                        <span style="width: 10px; 
                                     height: 10px; 
                                     background: #ef4444; 
                                     border-radius: 50%;
                                     display: inline-block;"></span>
                        <span style="color: white; 
                                     font-size: 0.9375rem; 
                                     font-weight: 700;">
                            API Service Offline
                        </span>
                    `;
                });
            
            topbar.style.position = 'relative';
            topbar.appendChild(status);
        }
    }

    // Additional enhancements
    function addTooltips() {
        const tryItOutButtons = document.querySelectorAll('.try-out__btn');
        tryItOutButtons.forEach(function(btn) {
            if (!btn.hasAttribute('title')) {
                btn.setAttribute('title', '? Click to test this endpoint with your own data');
                btn.style.cursor = 'pointer';
            }
        });

        const executeButtons = document.querySelectorAll('.btn.execute');
        executeButtons.forEach(function(btn) {
            if (!btn.hasAttribute('title')) {
                btn.setAttribute('title', '?? Send request to the API server');
                btn.style.cursor = 'pointer';
            }
        });
    }

    function enhanceCopyButtons() {
        document.addEventListener('click', function(e) {
            if (e.target.classList.contains('copy-to-clipboard')) {
                const button = e.target;
                const originalText = button.textContent;
                button.textContent = '? Copied!';
                button.style.background = '#22c55e';
                button.style.transform = 'scale(1.05)';
                
                setTimeout(function() {
                    button.textContent = originalText;
                    button.style.background = '';
                    button.style.transform = '';
                }, 2000);
            }
        });
    }

    function improveExamples() {
        setTimeout(function() {
            document.querySelectorAll('input[type="text"]:not([data-enhanced])').forEach(function(input) {
                if (!input.value && input.placeholder) {
                    input.setAttribute('data-enhanced', 'true');
                    const name = (input.name || input.placeholder).toLowerCase();
                    if (name.includes('customer') || name.includes('name')) {
                        input.placeholder = 'e.g., Ion Popescu';
                        input.setAttribute('title', 'Enter a customer name from the test data');
                    } else if (name.includes('product')) {
                        input.placeholder = 'e.g., Laptop Dell XPS 15';
                        input.setAttribute('title', 'Enter a product name from the catalog');
                    } else if (name.includes('email')) {
                        input.placeholder = 'e.g., customer@example.com';
                        input.setAttribute('title', 'Enter a valid email address');
                    } else if (name.includes('quantity')) {
                        input.placeholder = 'e.g., 1';
                        input.setAttribute('title', 'Enter the quantity (whole number)');
                    }
                }
            });
        }, 1500);
    }

    // Initialize all enhancements
    setTimeout(function() {
        addTooltips();
        enhanceCopyButtons();
        improveExamples();
    }, 1000);

    console.log('? Complete professional transformation applied!');
    console.log('?? Technical terms replaced with human-readable language');
    console.log('?? Beautiful styling applied (input fields protected)');
})();
